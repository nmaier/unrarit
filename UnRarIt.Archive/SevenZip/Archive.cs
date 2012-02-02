using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using UnRarIt.Interop;

namespace UnRarIt.Archive.SevenZip
{
    internal class SevenZipArchive : IDisposable, IInArchive
    {
#if DEBUG
        [DllImport("7z-x86-dbg.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateObject")]
        private extern static int CreateObject_32(ref Guid classID, ref Guid interfaceID, [MarshalAs(UnmanagedType.Interface)] out object outObject);

        [DllImport("7z-x86-dbg.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateObject")]
        private extern static int CreateObject_32SSE3(ref Guid classID, ref Guid interfaceID, [MarshalAs(UnmanagedType.Interface)] out object outObject);
#else
        [DllImport("7z-x86-generic.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateObject")]
        private extern static int CreateObject_32(ref Guid classID, ref Guid interfaceID, [MarshalAs(UnmanagedType.Interface)] out object outObject);

#if ENABLE_SSE3
        [DllImport("7z-x86-sse3.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateObject")]
#else
        [DllImport("7z-x86-generic.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateObject")]
#endif
        private extern static int CreateObject_32SSE3(ref Guid classID, ref Guid interfaceID, [MarshalAs(UnmanagedType.Interface)] out object outObject);
#endif

        [DllImport("7z-x64-generic.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateObject")]
        private extern static int CreateObject_64(ref Guid classID, ref Guid interfaceID, [MarshalAs(UnmanagedType.Interface)] out object outObject);

        [DllImport("7z-x64-sse3.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateObject")]
        private extern static int CreateObject_64SSE3(ref Guid classID, ref Guid interfaceID, [MarshalAs(UnmanagedType.Interface)] out object outObject);

        private IInArchive inArchive = null;
        private IInStream stream;
        internal SevenZipArchive(FileInfo aFile, IArchiveOpenCallback callback, Guid format)
        {
            stream = new SevenZipFileStream(aFile, FileMode.Open, FileAccess.Read);
            InternalOpen(callback, format);
        }
        internal SevenZipArchive(IInStream aStream, IArchiveOpenCallback callback, Guid format)
        {
            stream = aStream;
            InternalOpen(callback, format);
        }
        void InternalOpen(IArchiveOpenCallback callback, Guid format)
        {
            Guid Interface = typeof(IInArchive).GUID;
            object result;
            if (CpuInfo.isX64)
            {
                if (CpuInfo.hasSSE3)
                {
                    CreateObject_64SSE3(ref format, ref Interface, out result);
                }
                else
                {
                    CreateObject_64(ref format, ref Interface, out result);
                }
            }
            else
            {
                if (CpuInfo.hasSSE3)
                {
                    CreateObject_32SSE3(ref format, ref Interface, out result);
                }
                else
                {
                    CreateObject_32(ref format, ref Interface, out result);
                }
            }
            if (result == null)
            {
                throw new COMException("Cannot create InArchive");
            }
            inArchive = result as IInArchive;

            UInt64 sp = 1 << 23;
            inArchive.Open(stream, ref sp, callback);

        }
        public void Dispose()
        {
            Close();
        }

        public void Open(IInStream stream, ref UInt64 maxCheckStartPosition, IArchiveOpenCallback openArchiveCallback)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            if (inArchive != null)
            {
                inArchive.Close();
                inArchive = null;
            }
            if (stream != null && stream is IDisposable)
            {
                ((IDisposable)stream).Dispose();
            }
            stream = null;
        }

        public uint GetNumberOfItems()
        {
            return inArchive.GetNumberOfItems();
        }

        public void GetProperty(UInt32 index, ItemPropId propID, ref PropVariant value)
        {
            inArchive.GetProperty(index, propID, ref value);
        }

        public Variant GetProperty(UInt32 Index, ItemPropId propId)
        {
            return new Variant(this, Index, propId);
        }

        public void Extract(UInt32[] indices, UInt32 numItems, ExtractMode testMode, IArchiveExtractCallback extractCallback)
        {
            inArchive.Extract(indices, numItems, testMode, extractCallback);
        }

        public void GetArchiveProperty(ItemPropId propID, ref PropVariant value)
        {
            inArchive.GetArchiveProperty(propID, ref value);
        }

        public Variant GetArchiveProperty(ItemPropId propId)
        {
            return new Variant(this, propId);
        }

        public uint GetNumberOfProperties()
        {
            return inArchive.GetNumberOfProperties();
        }

        public void GetPropertyInfo(UInt32 index, out string name, out ItemPropId propID, out ushort varType)
        {
            inArchive.GetPropertyInfo(index, out name, out propID, out varType);
        }

        public uint GetNumberOfArchiveProperties()
        {
            return inArchive.GetNumberOfArchiveProperties();
        }

        public void GetArchivePropertyInfo(UInt32 index, string name, out ItemPropId propID, out ushort varType)
        {
            inArchive.GetArchivePropertyInfo(index, name, out propID, out varType);
        }

        public ISequentialInStream GetStream(UInt32 index)
        {
            return (inArchive as IInArchiveGetStream).GetStream(index);
        }
    }
}
