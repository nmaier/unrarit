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
        [DllImport("7z-x86-generic.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateObject")]
        private extern static int CreateObject_32(ref Guid classID, ref Guid interfaceID, [MarshalAs(UnmanagedType.Interface)] out object outObject);

        [DllImport("7z-x86_sse3.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateObject")]
        private extern static int CreateObject_32SSE3(ref Guid classID, ref Guid interfaceID, [MarshalAs(UnmanagedType.Interface)] out object outObject);

        [DllImport("7z-x64-generic.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateObject")]
        private extern static int CreateObject_64(ref Guid classID, ref Guid interfaceID, [MarshalAs(UnmanagedType.Interface)] out object outObject);

        [DllImport("7z-x64_sse3.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CreateObject")]
        private extern static int CreateObject_64SSE3(ref Guid classID, ref Guid interfaceID, [MarshalAs(UnmanagedType.Interface)] out object outObject);

        private IInArchive inArchive = null;
        private SevenZipStream stream;
        private List<FileInfo> files;
        internal SevenZipArchive(List<FileInfo> aFiles, IArchiveOpenCallback callback, Guid format)
        {
            files = aFiles;
            Guid Interface = typeof(IInArchive).GUID;
            object result;
            if (CpuInfo.isX64)
            {
                if (CpuInfo.hasSSE3)
                {
                    CreateObject_64(ref format, ref Interface, out result);
                }
                else
                {
                    CreateObject_64SSE3(ref format, ref Interface, out result);
                }
            }
            else
            {
                if (CpuInfo.hasSSE3)
                {
                    CreateObject_32(ref format, ref Interface, out result);
                }
                else
                {
                    CreateObject_32SSE3(ref format, ref Interface, out result);
                }
            }
            if (result == null)
            {
                throw new COMException("Cannot create InArchive");
            }
            inArchive = result as IInArchive;

            if (files.Count == 1 || true)
            {
                stream = new SevenZipFileStream(files[0], FileMode.Open, FileAccess.Read);
            }
            else {
                stream = new SevenZipVolumeStream(files, FileMode.Open, FileAccess.Read);
            }
            ulong sp = 4194304;
            inArchive.Open(stream, ref sp, callback);

        }
        public void Dispose()
        {
            Close();
        }

        public void Open(IInStream stream, ref ulong maxCheckStartPosition, IArchiveOpenCallback openArchiveCallback)
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
            if (stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }

        public uint GetNumberOfItems()
        {
            return inArchive.GetNumberOfItems();
        }

        public void GetProperty(uint index, ItemPropId propID, ref PropVariant value)
        {
            inArchive.GetProperty(index, propID, ref value);
        }

        public Variant GetProperty(uint Index, ItemPropId propId)
        {
            return new Variant(this, Index, propId);
        }

        public void Extract(uint[] indices, uint numItems, ExtractMode testMode, IArchiveExtractCallback extractCallback)
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

        public void GetPropertyInfo(uint index, out string name, out ItemPropId propID, out ushort varType)
        {
            inArchive.GetPropertyInfo(index, out name, out propID, out varType);
        }

        public uint GetNumberOfArchiveProperties()
        {
            return inArchive.GetNumberOfArchiveProperties();
        }

        public void GetArchivePropertyInfo(uint index, string name, out ItemPropId propID, out ushort varType)
        {
            inArchive.GetArchivePropertyInfo(index, name, out propID, out varType);
        }

        public ISequentialInStream GetStream(uint index)
        {
            return (inArchive as IInArchiveGetStream).GetStream(index);
        }
    }
}
