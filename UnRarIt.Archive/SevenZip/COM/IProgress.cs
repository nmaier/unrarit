using System;
using System.Runtime.InteropServices;

namespace UnRarIt.Archive
{
  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000000050000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IProgress
  {
    void SetTotal(ulong total);
    void SetCompleted([In] ref ulong completeValue);
  }
}
