using System;
using System.Runtime.InteropServices;

namespace UnRarIt.Archive
{
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
}
