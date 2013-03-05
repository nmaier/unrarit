using System;
using System.Runtime.InteropServices;

namespace UnRarIt.Archive
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable"),
  StructLayout(LayoutKind.Explicit)]
  internal struct PropVariantUnion
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
    [FieldOffset(0)]
    public byte byteValue;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
    [FieldOffset(0)]
    public IntPtr pointerValue;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
    [FieldOffset(0)]
    public IntPtr bstrValue;
    [FieldOffset(0)]
    public long longValue;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
    [FieldOffset(0)]
    public System.Runtime.InteropServices.ComTypes.FILETIME filetime;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
    [FieldOffset(0),
    MarshalAs(UnmanagedType.U8)]
    public ulong ui8Value;
  }
}
