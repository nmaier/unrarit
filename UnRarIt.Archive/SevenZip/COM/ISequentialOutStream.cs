using System;
using System.Runtime.InteropServices;

namespace UnRarIt.Archive
{
  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000300020000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface ISequentialOutStream
  {
    uint Write([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] data, uint size);
  }
}
