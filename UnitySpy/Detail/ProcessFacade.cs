namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using HackF5.UnitySpy.Util;
    using JetBrains.Annotations;

    /// <summary>
    /// A facade over a process that provides access to its memory space.
    /// </summary>
    [PublicAPI]
    public class ProcessFacade : IDisposable
    {
        /// <summary>
        /// Number of <c>ReadProcessMemory</c> syscalls issued since process start. This is NOT a headline
        /// performance metric (syscall count is an implementation detail); it exists only as a cheap sanity
        /// check so a benchmark can confirm that a change actually reduced the number of reads.
        /// </summary>
        public static long ReadProcessMemoryCallCount;

        /// <summary>
        /// Opt-in flag (default off) enabling Tier 1a "block reads": when a managed class instance is created its
        /// whole body is read once and subsequent primitive/pointer field reads are served from that buffer
        /// instead of one syscall per field. Off by default because it trades extra bytes per object for fewer
        /// syscalls, which is only a win when several fields of an object are actually read.
        /// </summary>
        public static bool UseBlockReads;

        // The currently active block-read window for the calling thread (see UseBlockReads). Reads whose address
        // range falls entirely inside the window are served from the buffer; everything else falls back to a
        // syscall, so correctness never depends on the window being present.
        [ThreadStatic]
        private static byte[] windowBuffer;

        [ThreadStatic]
        private static long windowBase;

        [ThreadStatic]
        private static int windowLength;

        private readonly MonoLibraryOffsets monoLibraryOffsets;
        private bool disposed;

        public ProcessFacade(int processId)
        {
            this.Process = Process.GetProcessById(processId);
            this.monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(Native.GetMainModuleFileName(this.Process));
        }

        public Process Process { get; }

        public void Dispose()
        {
            if (!disposed)
            {
                Process?.Dispose();
                disposed = true;
            }
        }

        public MonoLibraryOffsets MonoLibraryOffsets => this.monoLibraryOffsets;

        public int SizeOfPtr => this.monoLibraryOffsets.Is64Bits ? 8 : 4;

        public bool Is64Bits => this.monoLibraryOffsets.Is64Bits;

        public string ReadAsciiString(IntPtr address, int maxSize = 1024)
        {
            return this.ReadBufferValue(address, maxSize, b => b.ToAsciiString());
        }

        public string ReadAsciiStringPtr(IntPtr address, int maxSize = 1024) =>
            this.ReadAsciiString(this.ReadPtr(address), maxSize);

        public int ReadInt32(IntPtr address) => this.ReadStruct<int>(address);

        public long ReadInt64(IntPtr address) => this.ReadStruct<long>(address);

        public object ReadManaged([NotNull] TypeInfo type, List<TypeInfo> genericTypeArguments, IntPtr address)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            switch (type.TypeCode)
            {
                case TypeCode.BOOLEAN:
                    return this.ReadStruct<byte>(address) != 0;

                case TypeCode.CHAR:
                    return this.ReadStruct<char>(address);

                case TypeCode.I1:
                    return this.ReadStruct<byte>(address);

                case TypeCode.U1:
                    return unchecked((sbyte)this.ReadStruct<byte>(address));

                case TypeCode.I2:
                    return this.ReadStruct<short>(address);

                case TypeCode.U2:
                    return this.ReadStruct<ushort>(address);

                case TypeCode.I:
                case TypeCode.I4:
                    return this.ReadInt32(address);

                case TypeCode.U:
                case TypeCode.U4:
                    return this.ReadUInt32(address);

                case TypeCode.I8:
                    return this.ReadInt64(address);

                case TypeCode.U8:
                    return this.ReadUInt64(address);

                case TypeCode.R4:
                    return this.ReadStruct<float>(address);

                case TypeCode.R8:
                    return this.ReadStruct<double>(address);

                case TypeCode.STRING:
                    return this.ReadManagedString(address);

                case TypeCode.SZARRAY:
                    return this.ReadManagedArray(type, genericTypeArguments, address);

                case TypeCode.VALUETYPE:
                    try
                    {
                        return this.ReadManagedStructInstance(type, genericTypeArguments, address);
                    } catch (Exception e)
                    {
                        return this.ReadInt32(address);
                    }

                case TypeCode.CLASS:
                    return this.ReadManagedClassInstance(type, genericTypeArguments, address);

                case TypeCode.GENERICINST:
                    return this.ReadManagedGenericObject(type, genericTypeArguments, address);

                //// this is the type code for generic structs class-internals.h_MonoGenericParam. Good luck with
                //// that!
                //// Using the Generic Object works in at least some cases, like
                //// when retrieving the NetCache service.
                //// It's probably better to have something incomplete here
                //// that will raise an exception later on than throwing the exception right away?
                case TypeCode.OBJECT:
                    return this.ReadManagedGenericObject(type, genericTypeArguments, address);

                case TypeCode.VAR:
                    return this.ReadManagedVar(type, genericTypeArguments, address);

                // may need supporting
                case TypeCode.ARRAY:
                case TypeCode.ENUM:
                case TypeCode.MVAR:

                //// junk
                case TypeCode.END:
                case TypeCode.VOID:
                case TypeCode.PTR:
                case TypeCode.BYREF:
                case TypeCode.TYPEDBYREF:
                case TypeCode.FNPTR:
                case TypeCode.CMOD_REQD:
                case TypeCode.CMOD_OPT:
                case TypeCode.INTERNAL:
                case TypeCode.MODIFIER:
                case TypeCode.SENTINEL:
                case TypeCode.PINNED:
                    throw new ArgumentException($"Cannot read values of type '{type.TypeCode}'.");

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type),
                        type,
                        $"Cannot read unknown data type '{type.TypeCode}'.");
            }
        }

        /// <summary>
        /// Reads <paramref name="buffer"/>.Length bytes from the target process at <paramref name="address"/>
        /// in a single syscall. Used to snapshot a whole object body for block reads.
        /// </summary>
        public void ReadBlock([NotNull] byte[] buffer, IntPtr address) => this.ReadProcessMemory(buffer, address);

        /// <summary>
        /// Activates a block-read window covering <paramref name="buffer"/> mapped to target address
        /// <paramref name="baseAddress"/>, returning the previously active window so it can be restored. The
        /// window is thread-local and only consulted while <see cref="UseBlockReads"/> instances are read.
        /// </summary>
        internal static void EnterReadWindow(
            byte[] buffer,
            IntPtr baseAddress,
            out byte[] previousBuffer,
            out long previousBase,
            out int previousLength)
        {
            previousBuffer = windowBuffer;
            previousBase = windowBase;
            previousLength = windowLength;

            windowBuffer = buffer;
            windowBase = baseAddress.ToInt64();
            windowLength = buffer?.Length ?? 0;
        }

        internal static void ExitReadWindow(byte[] previousBuffer, long previousBase, int previousLength)
        {
            windowBuffer = previousBuffer;
            windowBase = previousBase;
            windowLength = previousLength;
        }

        private static bool TryGetWindow(IntPtr address, int size, out byte[] buffer, out int offset)
        {
            buffer = windowBuffer;
            offset = 0;
            if (buffer == null)
            {
                return false;
            }

            var relative = address.ToInt64() - windowBase;
            if (relative < 0 || relative + size > windowLength)
            {
                return false;
            }

            offset = (int)relative;
            return true;
        }

        public byte[] ReadModule([NotNull] ModuleInfo monoModuleInfo)
        {
            if (monoModuleInfo == null)
            {
                throw new ArgumentNullException(nameof(monoModuleInfo));
            }

            var buffer = new byte[monoModuleInfo.Size];
            this.ReadProcessMemory(buffer, monoModuleInfo.BaseAddress);
            return buffer;
        }

        public IntPtr ReadPtr(IntPtr address) => (IntPtr)(this.Is64Bits ? this.ReadUInt64(address) : this.ReadUInt32(address));

        public uint ReadUInt32(IntPtr address) => this.ReadStruct<uint>(address);

        public ulong ReadUInt64(IntPtr address) => this.ReadStruct<ulong>(address);

        public byte ReadByte(IntPtr address) => this.ReadStruct<byte>(address);

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesRead);

        /// <summary>
        /// Reads a single unmanaged value directly from the target process into a stack local, avoiding the
        /// byte[] rental and the per-call <see cref="GCHandle.Alloc(object, GCHandleType)"/> pinning that
        /// <see cref="ReadBufferValue{TValue}"/> incurs. This is the hot path for ints, pointers, etc.
        /// Only our own stack memory is written to; the target process is still only ever read from.
        /// </summary>
        private unsafe T ReadStruct<T>(IntPtr address)
            where T : unmanaged
        {
            if (TryGetWindow(address, sizeof(T), out var windowed, out var windowOffset))
            {
                fixed (byte* p = &windowed[windowOffset])
                {
                    return *(T*)p;
                }
            }

            T value = default;
            this.ReadProcessMemory((byte*)&value, address, sizeof(T));
            return value;
        }

        private unsafe void ReadProcessMemory(byte* buffer, IntPtr address, int size)
        {
            System.Threading.Interlocked.Increment(ref ProcessFacade.ReadProcessMemoryCallCount);
            if (!ProcessFacade.ReadProcessMemory(this.Process.Handle, address, (IntPtr)buffer, size, out _))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private TValue ReadBufferValue<TValue>(IntPtr address, int size, Func<byte[], TValue> read)
        {
            var buffer = ByteArrayPool.Instance.Rent(size);

            try
            {
                this.ReadProcessMemory(buffer, address, size: size);
                return read(buffer);
            }
            finally
            {
                ByteArrayPool.Instance.Return(buffer);
            }
        }

        private object ReadManagedArray(TypeInfo type, List<TypeInfo> genericTypeArguments, IntPtr address)
        {
            var ptr = this.ReadPtr(address);
            if (ptr == Constants.NullPtr)
            {
                return default;
            }

            var vtable = this.ReadPtr(ptr);
            var arrayDefinitionPtr = this.ReadPtr(vtable);
            var arrayDefinition = type.Image.GetTypeDefinition(arrayDefinitionPtr);
            var elementDefinition = type.Image.GetTypeDefinition(this.ReadPtr(arrayDefinitionPtr));

            var count = this.ReadInt32(ptr + SizeOfPtr * 3);
            var start = ptr + (SizeOfPtr * 4);
            if (count <= 0)
            {
                return new object[count < 0 ? 0 : count];
            }

            var stride = arrayDefinition.Size;

            // Fast path: for arrays of primitives, read the whole body in a single ReadProcessMemory call and
            // parse the elements locally, rather than issuing one syscall per element. Returns a typed array so
            // the values are not boxed. Reference/struct/string element arrays keep the per-element path because
            // those elements are live memory objects that must be read on access.
            if (this.TryReadPrimitiveArray(elementDefinition.TypeInfo.TypeCode, start, count, stride, out var primitiveArray))
            {
                return primitiveArray;
            }

            var result = new object[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = elementDefinition.TypeInfo.GetValue(genericTypeArguments, start + (i * stride));
            }

            return result;
        }

        private bool TryReadPrimitiveArray(TypeCode elementTypeCode, IntPtr start, int count, int stride, out object array)
        {
            array = null;
            switch (elementTypeCode)
            {
                case TypeCode.BOOLEAN:
                case TypeCode.CHAR:
                case TypeCode.I1:
                case TypeCode.U1:
                case TypeCode.I2:
                case TypeCode.U2:
                case TypeCode.I:
                case TypeCode.U:
                case TypeCode.I4:
                case TypeCode.U4:
                case TypeCode.I8:
                case TypeCode.U8:
                case TypeCode.R4:
                case TypeCode.R8:
                    break;
                default:
                    return false;
            }

            if (stride <= 0 || (long)count * stride > int.MaxValue)
            {
                return false;
            }

            var body = new byte[count * stride];
            this.ReadProcessMemory(body, start);

            switch (elementTypeCode)
            {
                case TypeCode.BOOLEAN:
                    {
                        var a = new bool[count];
                        for (var i = 0; i < count; i++) { a[i] = body[i * stride] != 0; }
                        array = a;
                        return true;
                    }

                case TypeCode.CHAR:
                    {
                        var a = new char[count];
                        for (var i = 0; i < count; i++) { a[i] = BitConverter.ToChar(body, i * stride); }
                        array = a;
                        return true;
                    }

                case TypeCode.I1:
                    {
                        var a = new byte[count];
                        for (var i = 0; i < count; i++) { a[i] = body[i * stride]; }
                        array = a;
                        return true;
                    }

                case TypeCode.U1:
                    {
                        var a = new sbyte[count];
                        for (var i = 0; i < count; i++) { a[i] = unchecked((sbyte)body[i * stride]); }
                        array = a;
                        return true;
                    }

                case TypeCode.I2:
                    {
                        var a = new short[count];
                        for (var i = 0; i < count; i++) { a[i] = BitConverter.ToInt16(body, i * stride); }
                        array = a;
                        return true;
                    }

                case TypeCode.U2:
                    {
                        var a = new ushort[count];
                        for (var i = 0; i < count; i++) { a[i] = BitConverter.ToUInt16(body, i * stride); }
                        array = a;
                        return true;
                    }

                case TypeCode.I:
                case TypeCode.I4:
                    {
                        var a = new int[count];
                        for (var i = 0; i < count; i++) { a[i] = BitConverter.ToInt32(body, i * stride); }
                        array = a;
                        return true;
                    }

                case TypeCode.U:
                case TypeCode.U4:
                    {
                        var a = new uint[count];
                        for (var i = 0; i < count; i++) { a[i] = BitConverter.ToUInt32(body, i * stride); }
                        array = a;
                        return true;
                    }

                case TypeCode.I8:
                    {
                        var a = new long[count];
                        for (var i = 0; i < count; i++) { a[i] = BitConverter.ToInt64(body, i * stride); }
                        array = a;
                        return true;
                    }

                case TypeCode.U8:
                    {
                        var a = new ulong[count];
                        for (var i = 0; i < count; i++) { a[i] = BitConverter.ToUInt64(body, i * stride); }
                        array = a;
                        return true;
                    }

                case TypeCode.R4:
                    {
                        var a = new float[count];
                        for (var i = 0; i < count; i++) { a[i] = BitConverter.ToSingle(body, i * stride); }
                        array = a;
                        return true;
                    }

                case TypeCode.R8:
                    {
                        var a = new double[count];
                        for (var i = 0; i < count; i++) { a[i] = BitConverter.ToDouble(body, i * stride); }
                        array = a;
                        return true;
                    }

                default:
                    return false;
            }
        }

        private ManagedClassInstance ReadManagedClassInstance(TypeInfo type, List<TypeInfo> genericTypeArguments, IntPtr address)
        {
            var ptr = this.ReadPtr(address);
            return ptr == Constants.NullPtr
                ? default
                : new ManagedClassInstance(type.Image, genericTypeArguments, ptr);
        }

        private object ReadManagedGenericObject(TypeInfo type, List<TypeInfo> genericTypeArguments, IntPtr address)
        {
            var genericDefinition = type.Image.GetTypeDefinition(this.ReadPtr(type.Data));
            if (genericDefinition.IsValueType)
            {
                return new ManagedStructInstance(genericDefinition, genericTypeArguments, address);
            }

            return this.ReadManagedClassInstance(type, genericTypeArguments, address);
        }

        private object ReadManagedVar(TypeInfo type, List<TypeInfo> genericTypeArguments, IntPtr address)
        {
            var monoGenericParamPtr = type.Data;
            var monoGenericParamAddress = this.ReadPtr(monoGenericParamPtr);

            //// we need to move three pointer sizes to get to the MonoClass Pointer
            // See _MonoGenericContainer
            // (https://github.com/Unity-Technologies/mono/blob/unity-master/mono/metadata/class-internals.h)
            // var genericDefinitionPtr = monoGenericContainerAddress + (3 * this.SizeOfPtr);
            // var genericDefinition = type.Image.GetTypeDefinition(this.ReadPtr(genericDefinitionPtr));

            int numberOfGenericArgument = this.ReadInt32(monoGenericParamPtr + this.SizeOfPtr);

            int offset = 0;
            for (int i = 0; i < numberOfGenericArgument; i++)
            {
                offset += this.GetSize(genericTypeArguments[i].TypeCode) - this.SizeOfPtr;
            }

            var genericArgumentType = genericTypeArguments[numberOfGenericArgument];
            return this.ReadManaged(genericArgumentType, null, address + offset);
        }

        private string ReadManagedString(IntPtr address)
        {
            var ptr = this.ReadPtr(address);
            if (ptr == Constants.NullPtr)
            {
                return default;
            }

            var length = this.ReadInt32(ptr + SizeOfPtr * 2);
            var byteCount = 2 * length;

            // Inlined (rather than ReadBufferValue with a lambda) so the decode does not capture `length` into a
            // per-call closure on this hot path.
            var buffer = ByteArrayPool.Instance.Rent(byteCount);
            try
            {
                this.ReadProcessMemory(buffer, ptr + MonoLibraryOffsets.UnicodeString, size: byteCount);
                return Encoding.Unicode.GetString(buffer, 0, byteCount);
            }
            finally
            {
                ByteArrayPool.Instance.Return(buffer);
            }
        }

        private object ReadManagedStructInstance(TypeInfo type, List<TypeInfo> genericTypeArguments, IntPtr address)
        {
            var definition = type.Image.GetTypeDefinition(type.Data);
            var obj = new ManagedStructInstance(definition, genericTypeArguments, address);
            //var t = obj.GetValue<object>("enumSeperator");
            return obj.TypeDefinition.IsEnum ? obj.GetValue<object>("value__") : obj;
        }


        private int GetSize(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.BOOLEAN:
                    return sizeof(bool);

                case TypeCode.CHAR:
                    return sizeof(char);

                case TypeCode.I1:
                    return sizeof(byte);

                case TypeCode.U1:
                    return sizeof(sbyte);

                case TypeCode.I2:
                    return sizeof(short);

                case TypeCode.U2:
                    return sizeof(ushort);

                case TypeCode.I:
                case TypeCode.I4:
                    return sizeof(int);

                case TypeCode.U:
                case TypeCode.U4:
                    return sizeof(uint);

                case TypeCode.I8:
                    return sizeof(long);

                case TypeCode.U8:
                    return sizeof(ulong);

                case TypeCode.R4:
                    return sizeof(float);

                case TypeCode.R8:
                    return sizeof(double);

                case TypeCode.STRING:
                case TypeCode.SZARRAY:
                case TypeCode.VALUETYPE:
                case TypeCode.CLASS:
                case TypeCode.GENERICINST:
                case TypeCode.OBJECT:
                case TypeCode.VAR:
                    return this.SizeOfPtr;

                // may need supporting
                case TypeCode.ARRAY:
                case TypeCode.ENUM:
                case TypeCode.MVAR:

                //// junk
                case TypeCode.END:
                case TypeCode.VOID:
                case TypeCode.PTR:
                case TypeCode.BYREF:
                case TypeCode.TYPEDBYREF:
                case TypeCode.FNPTR:
                case TypeCode.CMOD_REQD:
                case TypeCode.CMOD_OPT:
                case TypeCode.INTERNAL:
                case TypeCode.MODIFIER:
                case TypeCode.SENTINEL:
                case TypeCode.PINNED:
                    throw new ArgumentException($"Cannot get size of types '{typeCode}'.");

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(typeCode),
                        typeCode,
                        $"Cannot get size of unknown data type '{typeCode}'.");
            }
        }

        private void ReadProcessMemory(
            byte[] buffer,
            uint processAddress,
            bool allowPartialRead = false,
            int? size = default)
            => this.ReadProcessMemory(buffer, new IntPtr(processAddress), allowPartialRead, size);

        private void ReadProcessMemory(
            byte[] buffer,
            IntPtr processAddress,
            bool allowPartialRead = false,
            int? size = default)
        {
            var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                System.Threading.Interlocked.Increment(ref ProcessFacade.ReadProcessMemoryCallCount);
                var bufferPointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
                if (!ProcessFacade.ReadProcessMemory(
                    this.Process.Handle,
                    processAddress,
                    bufferPointer,
                    size ?? buffer.Length,
                    out _))
                {
                    var error = Marshal.GetLastWin32Error();
                    if ((error == 299) && allowPartialRead)
                    {
                        return;
                    }

                    throw new Win32Exception(error);
                }
            }
            finally
            {
                bufferHandle.Free();
            }
        }
    }
}