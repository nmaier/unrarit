using System;
using System.Runtime.InteropServices;

namespace UnRarIt.Archive
{
  internal class Variant : IDisposable
  {
    private PropVariant variant = new PropVariant();


    internal Variant(IInArchive Archive, ItemPropId Id)
    {
      Archive.GetArchiveProperty(Id, ref variant);
    }
    internal Variant(IInArchive Archive, uint FileNumber, ItemPropId Id)
    {
      Archive.GetProperty(FileNumber, Id, ref variant);
    }


    public VarEnum VarType
    {
      get {
        return variant.type;
      }
    }

    internal object GetObject()
    {
      switch (VarType) {
        case VarEnum.VT_EMPTY:
          return null;
        case VarEnum.VT_FILETIME:
          return DateTime.FromFileTime(variant.union.longValue);
        default:
          var PropHandle = GCHandle.Alloc(variant, GCHandleType.Pinned);
          try {
            return Marshal.GetObjectForNativeVariant(PropHandle.AddrOfPinnedObject());
          }
          finally {
            PropHandle.Free();
          }
      }
    }


    public void Dispose()
    {
      switch (VarType) {
        case VarEnum.VT_EMPTY:
          break;
        case VarEnum.VT_NULL:
        case VarEnum.VT_I2:
        case VarEnum.VT_I4:
        case VarEnum.VT_R4:
        case VarEnum.VT_R8:
        case VarEnum.VT_CY:
        case VarEnum.VT_DATE:
        case VarEnum.VT_ERROR:
        case VarEnum.VT_BOOL:
        case VarEnum.VT_I1:
        case VarEnum.VT_UI1:
        case VarEnum.VT_UI2:
        case VarEnum.VT_UI4:
        case VarEnum.VT_I8:
        case VarEnum.VT_UI8:
        case VarEnum.VT_INT:
        case VarEnum.VT_UINT:
        case VarEnum.VT_HRESULT:
        case VarEnum.VT_FILETIME:
          variant.type = 0;
          break;
        default:
          SafeNativeMethods.PropVariantClear(ref variant);
          break;
      }
    }

    public bool GetBool()
    {
      var o = GetObject();
      return o == null ? false : (Boolean)o;
    }

    public string GetString()
    {
      return GetObject() as string;
    }

    public uint GetUint()
    {
      var o = GetObject();
      return o == null ? 0 : (UInt32)o;
    }

    public ulong GetUlong()
    {
      var o = GetObject();
      return o == null ? 0 : (UInt64)o;
    }
  }
}
