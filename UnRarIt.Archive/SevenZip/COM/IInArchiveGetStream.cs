using System;
using System.Runtime.InteropServices;

namespace UnRarIt.Archive
{
  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000600400000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IInArchiveGetStream
  {
    [return: MarshalAs(UnmanagedType.Interface)]
    ISequentialInStream GetStream(uint index);
  }
}
