using System;
using System.IO;
using System.Runtime.InteropServices;
using UnRarIt.Interop;

namespace UnRarIt.Archive
{
  internal class Archive : IDisposable, IInArchive
  {
    private IInArchive inArchive = null;

    private IInStream stream;


    internal Archive(FileInfo aFile, IArchiveOpenCallback callback, Guid format)
    {
      stream = new SevenZipFileStream(aFile, FileMode.Open, FileAccess.Read);
      InternalOpen(callback, format);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "UnRarIt.Archive.SafeNativeMethods.CreateObject_64(System.Guid@,System.Guid@,System.Object@)"),
    System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "UnRarIt.Archive.SafeNativeMethods.CreateObject_32(System.Guid@,System.Guid@,System.Object@)")]
    private void InternalOpen(IArchiveOpenCallback callback, Guid format)
    {
      var Interface = typeof(IInArchive).GUID;
      object result;
      if (CpuInfo.IsX64) {
        SafeNativeMethods.CreateObject_64(ref format, ref Interface, out result);
      }
      else {
        SafeNativeMethods.CreateObject_32(ref format, ref Interface, out result);
      }
      if (result == null) {
        throw new COMException("Cannot create Archive");
      }
      inArchive = result as IInArchive;

      var sp = (ulong)(1 << 23);
      inArchive.Open(stream, ref sp, callback);
    }


    public void Close()
    {
      if (inArchive != null) {
        inArchive.Close();
        inArchive = null;
      }
      if (stream != null && stream is IDisposable) {
        ((IDisposable)stream).Dispose();
      }
      stream = null;
    }

    public void Dispose()
    {
      Close();
    }

    public void Extract(UInt32[] indices, UInt32 numItems, ExtractMode testMode, IArchiveExtractCallback extractCallback)
    {
      inArchive.Extract(indices, numItems, testMode, extractCallback);
    }

    public Variant GetArchiveProperty(ItemPropId propId)
    {
      return new Variant(this, propId);
    }

    public void GetArchiveProperty(ItemPropId propID, ref PropVariant value)
    {
      inArchive.GetArchiveProperty(propID, ref value);
    }

    public void GetArchivePropertyInfo(UInt32 index, string name, out ItemPropId propID, out ushort varType)
    {
      inArchive.GetArchivePropertyInfo(index, name, out propID, out varType);
    }

    public uint GetNumberOfArchiveProperties()
    {
      return inArchive.GetNumberOfArchiveProperties();
    }

    public uint GetNumberOfItems()
    {
      return inArchive.GetNumberOfItems();
    }

    public uint GetNumberOfProperties()
    {
      return inArchive.GetNumberOfProperties();
    }

    public Variant GetProperty(UInt32 Index, ItemPropId propId)
    {
      return new Variant(this, Index, propId);
    }

    public void GetProperty(UInt32 index, ItemPropId propID, ref PropVariant value)
    {
      inArchive.GetProperty(index, propID, ref value);
    }

    public void GetPropertyInfo(UInt32 index, out string name, out ItemPropId propID, out ushort varType)
    {
      inArchive.GetPropertyInfo(index, out name, out propID, out varType);
    }

    public ISequentialInStream GetStream(UInt32 index)
    {
      return (inArchive as IInArchiveGetStream).GetStream(index);
    }

    public void Open(IInStream s, ref UInt64 maxCheckStartPosition, IArchiveOpenCallback openArchiveCallback)
    {
      throw new NotImplementedException();
    }
  }
}
