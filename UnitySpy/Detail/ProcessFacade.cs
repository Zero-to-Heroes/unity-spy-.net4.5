﻿namespace HackF5.UnitySpy.Detail
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
    public class ProcessFacade
    {
        private readonly MonoLibraryOffsets monoLibraryOffsets;

        public ProcessFacade(int processId)
        {
            this.Process = Process.GetProcessById(processId);
            this.monoLibraryOffsets = MonoLibraryOffsets.GetOffsets(Native.GetMainModuleFileName(this.Process));
        }

        public Process Process { get; }

        public MonoLibraryOffsets MonoLibraryOffsets => this.monoLibraryOffsets;

        public int SizeOfPtr => this.monoLibraryOffsets.Is64Bits ? 8 : 4;

        public bool Is64Bits => this.monoLibraryOffsets.Is64Bits;

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

        public object ReadManaged([NotNull] TypeInfo type, List<TypeInfo> genericTypeArguments, IntPtr address)
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

        private object[] ReadManagedArray(TypeInfo type, List<TypeInfo> genericTypeArguments, IntPtr address)
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
            var result = new object[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = elementDefinition.TypeInfo.GetValue(genericTypeArguments, start + (i * arrayDefinition.Size));
            }

            return result;
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

            return this.ReadBufferValue(
                ptr + MonoLibraryOffsets.UnicodeString,
                2 * length,
                b => Encoding.Unicode.GetString(b, 0, 2 * length));
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
                case TypeCode.R8:
                    return sizeof(char);

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