using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace UnRarIt.Archive.SevenZip
{
    #region Interfaces
    [ComImport]
    [Guid("23170F69-40C1-278A-0000-000000050000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IProgress
    {
        void SetTotal(ulong total);
        void SetCompleted([In] ref ulong completeValue);
    }

    [ComImport]
    [Guid("23170F69-40C1-278A-0000-000600100000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IArchiveOpenCallback
    {
        void SetTotal(IntPtr files, IntPtr bytes);
        void SetCompleted(IntPtr files, IntPtr bytes);
    }

    [ComImport]
    [Guid("23170F69-40C1-278A-0000-000500100000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IGetPassword
    {
        void CryptoGetTextPassword([MarshalAs(UnmanagedType.BStr)] out string password);
    }

    internal enum ExtractMode : int
    {
        Extract = 0,
        Test,
        Skip
    }

    internal enum OperationResult : int
    {
        OK = 0,
        UnSupportedMethod,
        DataError,
        CRCError
    }

    [ComImport]
    [Guid("23170F69-40C1-278A-0000-000600200000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IArchiveExtractCallback
    {
        void SetTotal(UInt64 total);
        void SetCompleted([In] ref UInt64 completeValue);

        void GetStream(UInt32 index, [MarshalAs(UnmanagedType.Interface)] out ISequentialOutStream outStream, ExtractMode askExtractMode);

        void PrepareOperation(ExtractMode extractMode);
        void SetOperationResult(OperationResult result);
    }

    [ComImport]
    [Guid("23170F69-40C1-278A-0000-000600400000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IInArchiveGetStream
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        ISequentialInStream GetStream(uint index);
    }

    [ComImport]
    [Guid("23170F69-40C1-278A-0000-000300010000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISequentialInStream
    {
        uint Read([Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] data, uint size);
    }

    [ComImport]
    [Guid("23170F69-40C1-278A-0000-000300020000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISequentialOutStream
    {
        uint Write([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] data, uint size);
    }

    [ComImport]
    [Guid("23170F69-40C1-278A-0000-000300030000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IInStream
    {
        uint Read([Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] data, uint size);

        ulong Seek(long offset, uint seekOrigin);
    }

    [ComImport]
    [Guid("23170F69-40C1-278A-0000-000600600000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IInArchive
    {
        void Open(IInStream stream, ref UInt64 maxCheckStartPosition, [MarshalAs(UnmanagedType.Interface)] IArchiveOpenCallback openArchiveCallback);

        void Close();

        UInt32 GetNumberOfItems();

        void GetProperty(UInt32 index, ItemPropId propID, ref PropVariant value);

        void Extract([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] UInt32[] indices, UInt32 numItems, ExtractMode testMode, [MarshalAs(UnmanagedType.Interface)] IArchiveExtractCallback extractCallback);

        void GetArchiveProperty(ItemPropId propID, ref PropVariant value);

        uint GetNumberOfProperties();
        void GetPropertyInfo(UInt32 index, [MarshalAs(UnmanagedType.BStr)] out string name, out ItemPropId propID, out ushort varType);

        uint GetNumberOfArchiveProperties();
        void GetArchivePropertyInfo(UInt32 index, [MarshalAs(UnmanagedType.BStr)] string name, out ItemPropId propID, out ushort varType);
    }

    [ComImport]
    [Guid("23170F69-40C1-278A-0000-000600300000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IArchiveOpenVolumeCallback
    {
        void GetProperty(ItemPropId propID, ref PropVariant rv);
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.I4)]
        int GetStream([MarshalAs(UnmanagedType.LPWStr)] string name, ref IInStream stream);
    }
    #endregion

    #region Props
    internal enum ArchivePropId : uint
    {
        Name = 0,
        ClassID,
        Extension,
        AddExtension,
        Update,
        KeepName,
        StartSignature,
        FinishSignature,
        Associate
    }

    internal enum ItemPropId : uint
    {
        NoProperty = 0,

        HandlerItemIndex = 2,
        Path,
        Name,
        Extension,
        IsDir,
        Size,
        PackedSize,
        Attrib,
        CreationTime,
        AccessTime,
        ModificationTime,
        Solid,
        Commented,
        Encrypted,
        SplitBefore,
        SplitAfter,
        DictionarySize,
        CRC,
        Type,
        IsAnti,
        Method,
        HostOS,
        FileSystem,
        User,
        Group,
        Block,
        Comment,
        Position,
        Prefix,
        NumSubDirs,
        NumSubFiles,
        UnpackVer,
        Volume,
        IsVolume,
        Offset,
        Links,
        NumBlocks,
        NumVolumes,
        TimeType,
        Bit64,
        BigEndian,
        Cpu,
        PhySize,
        HeadersSize,
        Checksum,
        Characts,
        Va,

        TotalSize = 0x1100,
        FreeSpace,
        ClusterSize,
        VolumeName,

        LocalName = 0x1200,
        Provider,

        UserDefined = 0x10000
    }
    #endregion

    #region Structs
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal struct PropVariant
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.U4)]
        public VarEnum type;
        [FieldOffset(8)]
        internal PropVariantUnion union;
    }
    [StructLayout(LayoutKind.Explicit)]
    internal struct PropVariantUnion
    {
        [FieldOffset(0)]
        public IntPtr pointerValue;
        [FieldOffset(0)]
        public byte byteValue;
        [FieldOffset(0)]
        public long longValue;
        [FieldOffset(0), MarshalAs(UnmanagedType.U8)]
        public ulong ui8Value;
        [FieldOffset(0)]
        public IntPtr bstrValue;
        [FieldOffset(0)]
        public System.Runtime.InteropServices.ComTypes.FILETIME filetime;
    }
    internal class Variant : IDisposable
    {
        [DllImport("ole32.dll")]
        private static extern int PropVariantClear(ref PropVariant pvar);

        private PropVariant variant = new PropVariant();

        internal Variant(IInArchive Archive, uint FileNumber, ItemPropId Id)
        {
            Archive.GetProperty(FileNumber, Id, ref variant);
        }
        internal Variant(IInArchive Archive, ItemPropId Id)
        {
            Archive.GetArchiveProperty(Id, ref variant);
        }

        public VarEnum VarType
        {
            get { return variant.type; }
        }

        public void Dispose()
        {
            switch (VarType)
            {
                case VarEnum.VT_EMPTY:
                    break;
                case VarEnum.VT_NULL:
                case VarEnum.VT_I2:
                case VarEnum.VT_I4:
                case VarEnum.VT_R4:
                case VarEnum.VT_R8:
                case VarEnum.VT_CY:
                case VarEnum.VT_DATE:
                case VarEnum.VT_ERROR:
                case VarEnum.VT_BOOL:
                case VarEnum.VT_I1:
                case VarEnum.VT_UI1:
                case VarEnum.VT_UI2:
                case VarEnum.VT_UI4:
                case VarEnum.VT_I8:
                case VarEnum.VT_UI8:
                case VarEnum.VT_INT:
                case VarEnum.VT_UINT:
                case VarEnum.VT_HRESULT:
                case VarEnum.VT_FILETIME:
                    variant.type = 0;
                    break;
                default:
                    PropVariantClear(ref variant);
                    break;
            }
        }

        internal object GetObject()
        {
            switch (VarType)
            {
                case VarEnum.VT_EMPTY:
                    return null;
                case VarEnum.VT_FILETIME:
                    return DateTime.FromFileTime(variant.union.longValue);
                default:
                    GCHandle PropHandle = GCHandle.Alloc(variant, GCHandleType.Pinned);
                    try
                    {
                        return Marshal.GetObjectForNativeVariant(PropHandle.AddrOfPinnedObject());
                    }
                    finally
                    {
                        PropHandle.Free();
                    }
            }
        }

        public string GetString()
        {
            return GetObject() as string;
        }
        public bool GetBool()
        {
            object o = GetObject();
            return o == null ? false : (Boolean)o;
        }
        public int GetInt()
        {
            object o = GetObject();
            return o == null ? 0 : (Int32)o;
        }
        public uint GetUint()
        {
            object o = GetObject();
            return o == null ? 0 : (UInt32)o;

        }
        public long GetLong()
        {
            object o = GetObject();
            return o == null ? 0 : (Int64)o;
        }
        public ulong GetUlong()
        {
            object o = GetObject();
            return o == null ? 0 : (UInt64)o;
        }
    }
    #endregion
}
