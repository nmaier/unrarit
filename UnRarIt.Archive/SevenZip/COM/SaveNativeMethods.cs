using System;
using System.Runtime.InteropServices;

namespace UnRarIt.Archive
{
  internal static class SafeNativeMethods
  {
    [DllImport("7z-x86.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateObject")]
    internal extern static int CreateObject_32(ref Guid classID, ref Guid interfaceID, [MarshalAs(UnmanagedType.Interface)] out object outObject);

    [DllImport("7z-amd64.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateObject")]
    internal extern static int CreateObject_64(ref Guid classID, ref Guid interfaceID, [MarshalAs(UnmanagedType.Interface)] out object outObject);

    [DllImport("ole32.dll")]
    internal static extern int PropVariantClear(ref PropVariant pvar);
  }
}
