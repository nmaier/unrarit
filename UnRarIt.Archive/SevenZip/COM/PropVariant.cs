using System.Runtime.InteropServices;

namespace UnRarIt.Archive
{
  [StructLayout(LayoutKind.Explicit, Size = 16)]
  internal struct PropVariant
  {
    [FieldOffset(0)]
    [MarshalAs(UnmanagedType.U4)]
    public VarEnum type;

    [FieldOffset(8)]
    internal PropVariantUnion union;
  }
}
