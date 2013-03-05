using System;
using System.Runtime.InteropServices;

namespace UnRarIt.Archive
{
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
}
