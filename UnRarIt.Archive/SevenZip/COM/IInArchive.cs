using System;
using System.Runtime.InteropServices;

namespace UnRarIt.Archive
{
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
}
