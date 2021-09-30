﻿namespace HackF5.UnitySpy.Detail
{
    using System;
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
    public class ProcessFacade
    {
        private readonly bool is64Bits;

        public ProcessFacade(int processId)
        {
            this.Process = Process.GetProcessById(processId);
            is64Bits = Native.IsWow64Process(this.Process);

            string mainModulePath = Native.GetMainModuleFileName(this.Process);

            FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(mainModulePath);
            string unityVersion = myFileVersionInfo.FileVersion;
            this.MonoLibraryOffsets = MonoLibraryOffsets.GetOffsets(unityVersion, is64Bits);
        }

        public Process Process { get; }

        public MonoLibraryOffsets MonoLibraryOffsets { get; }

        public int SizeOfPtr => is64Bits ? 8 : 4;

        public bool Is64Bits => is64Bits;

        public string ReadAsciiString(IntPtr address, int maxSize = 1024)
        {
            return this.ReadBufferValue(address, maxSize, b => b.ToAsciiString());
        }

        public string ReadAsciiStringPtr(IntPtr address, int maxSize = 1024) =>
            this.ReadAsciiString(this.ReadPtr(address), maxSize);

        public int ReadInt32(IntPtr address)
        {
            return this.ReadBufferValue(address, sizeof(int), b => b.ToInt32());
        }
        public long ReadInt64(IntPtr address)
        {
            return this.ReadBufferValue(address, sizeof(long), b => b.ToInt64());
        }

        public object ReadManaged([NotNull] TypeInfo type, IntPtr address)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            switch (type.TypeCode)
            {
                case TypeCode.BOOLEAN:
                    return this.ReadBufferValue(address, 1, b => b[0] != 0);

                case TypeCode.CHAR:
                    return this.ReadBufferValue(address, sizeof(char), ConversionUtils.ToChar);

                case TypeCode.I1:
                    return this.ReadBufferValue(address, sizeof(byte), b => b[0]);

                case TypeCode.U1:
                    return this.ReadBufferValue(address, sizeof(sbyte), b => unchecked((sbyte)b[0]));

                case TypeCode.I2:
                    return this.ReadBufferValue(address, sizeof(short), ConversionUtils.ToInt16);

                case TypeCode.U2:
                    return this.ReadBufferValue(address, sizeof(ushort), ConversionUtils.ToUInt16);

                case TypeCode.I:
                case TypeCode.I4:
                    return this.ReadInt32(address);

                case TypeCode.U:
                case TypeCode.U4:
                    return this.ReadUInt32(address);

                case TypeCode.I8:
                    return this.ReadInt64(address);
                    //return this.ReadBufferValue(address, sizeof(char), ConversionUtils.ToInt64);

                case TypeCode.U8:
                    return this.ReadUInt64(address);
                    //return this.ReadBufferValue(address, sizeof(char), ConversionUtils.ToUInt64);

                case TypeCode.R4:
                    return this.ReadBufferValue(address, sizeof(char), ConversionUtils.ToSingle);

                case TypeCode.R8:
                    return this.ReadBufferValue(address, sizeof(char), ConversionUtils.ToDouble);

                case TypeCode.STRING:
                    return this.ReadManagedString(address);

                case TypeCode.SZARRAY:
                    return this.ReadManagedArray(type, address);

                case TypeCode.VALUETYPE:
                    try
                    {
                        return this.ReadManagedStructInstance(type, address);
                    } catch (Exception e)
                    {
                        return this.ReadInt32(address);
                    }

                case TypeCode.CLASS:
                    return this.ReadManagedClassInstance(type, address);

                case TypeCode.GENERICINST:
                    return this.ReadManagedGenericObject(type, address);

                //// this is the type code for generic structs class-internals.h_MonoGenericParam. Good luck with
                //// that!
                //// Using the Generic Object works in at least some cases, like
                //// when retrieving the NetCache service.
                //// It's probably better to have something incomplete here
                //// that will raise an exception later on than throwing the exception right away?
                case TypeCode.OBJECT:
                    return this.ReadManagedGenericObject(type, address);

                case TypeCode.VAR:
                    // Really not sure this is the way to do it
                    return this.ReadInt32(address);

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

        public IntPtr ReadPtr(IntPtr address) => (IntPtr) (this.is64Bits ? this.ReadUInt64(address) : this.ReadUInt32(address));

        public uint ReadUInt32(IntPtr address)
        {
            return this.ReadBufferValue(address, sizeof(uint), b => b.ToUInt32());
        }

        public ulong ReadUInt64(IntPtr address)
        {
            return this.ReadBufferValue(address, sizeof(ulong), b => b.ToUInt64());
        }

        public byte ReadByte(IntPtr address)
        {
            return this.ReadBufferValue(address, sizeof(byte), b => b.ToByte());
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesRead);

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

        private object[] ReadManagedArray(TypeInfo type, IntPtr address)
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

            // Not sure why in the new version of unity the array it seems to have pointers instead of the element
            // Maybe mono changed?
            var elementSize = MonoLibraryOffsets.UsesArrayDefinitionSize ? arrayDefinition.Size : SizeOfPtr;

            var count = this.ReadInt32(ptr + SizeOfPtr * 3);
            var start = ptr + (SizeOfPtr * 4);
            var result = new object[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = elementDefinition.TypeInfo.GetValue(start + (i * elementSize));
            }

            return result;
        }

        private ManagedClassInstance ReadManagedClassInstance(TypeInfo type, IntPtr address)
        {
            var ptr = this.ReadPtr(address);
            return ptr == Constants.NullPtr
                ? default
                : new ManagedClassInstance(type.Image, ptr);
        }

        private object ReadManagedGenericObject(TypeInfo type, IntPtr address)
        {
            var genericDefinition = type.Image.GetTypeDefinition(this.ReadPtr(type.Data));
            if (genericDefinition.IsValueType)
            {
                return new ManagedStructInstance(genericDefinition, address);
            }

            return this.ReadManagedClassInstance(type, address);
        }

        private string ReadManagedString(IntPtr address)
        {
            var ptr = this.ReadPtr(address);
            if (ptr == Constants.NullPtr)
            {
                return default;
            }

            var length = this.ReadInt32(ptr + SizeOfPtr * 2);

            return this.ReadBufferValue(
                ptr + MonoLibraryOffsets.UnicodeString,
                2 * length,
                b => Encoding.Unicode.GetString(b, 0, 2 * length));
        }

        private object ReadManagedStructInstance(TypeInfo type, IntPtr address)
        {
            var definition = type.Image.GetTypeDefinition(type.Data);
            var obj = new ManagedStructInstance(definition, address);
            //var t = obj.GetValue<object>("enumSeperator");
            return obj.TypeDefinition.IsEnum ? obj.GetValue<object>("value__") : obj;
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