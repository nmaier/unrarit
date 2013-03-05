using System;
using System.Runtime.InteropServices;

namespace UnRarIt.Archive
{
  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000500100000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface IGetPassword
  {
    void CryptoGetTextPassword([MarshalAs(UnmanagedType.BStr)] out string password);
  }
}
