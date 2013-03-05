using System;
using System.Runtime.InteropServices;

namespace UnRarIt.Archive
{
  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000600100000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IArchiveOpenCallback
  {
    void SetTotal(IntPtr files, IntPtr bytes);
    void SetCompleted(IntPtr files, IntPtr bytes);
  }
}
