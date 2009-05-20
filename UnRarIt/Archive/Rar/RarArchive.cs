using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

namespace UnRarIt.Archive.Rar
{
    abstract internal class RarArchive : IDisposable
    {

        #region UnRarDLL

        [StructLayout(LayoutKind.Sequential)]
        protected struct RAROpenArchiveDataEx
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string dummy1;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string ArcName;
            public RarOpenMode OpenMode;
            public RarStatus OpenResult;
            [MarshalAs(UnmanagedType.LPStr)]
            public string CmtBuf;
            public uint CmtBufSize;
            public uint CmtSize;
            public uint CmtState;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public uint[] Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct Header
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string dummy1_DONOTUSE;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string ArcName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string dummy2_DONOTUSE;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string FileName;

            public uint Flags;
            public uint PackSize;
            public uint PackSizeHigh;
            public uint UnpSize;
            public uint UnpSizeHigh;
            public uint HostOS;
            public uint FileCRC;
            public uint FileTime;
            public uint UnpVer;
            public uint Method;
            public uint FileAttr;

            [MarshalAs(UnmanagedType.LPStr)]
            public string CmtBuf;
            public uint CmtBufSize;
            public uint CmtSize;
            public uint CmtState;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            public uint[] Reserved;
        };

        public delegate int UnRarCallback(RarMessage msg, IntPtr UserData, IntPtr WParam, IntPtr LParam);


        abstract protected RarStatus CloseArchive();
        abstract protected IntPtr OpenArchive(ref RAROpenArchiveDataEx ArchiveData);
        abstract public RarStatus GetHeader(ref Header HeaderData);
        abstract public RarStatus ProcessFile(RarOperation Operation, string DestPath, string DestName);
        abstract public RarStatus GetVersion();
        abstract public RarStatus SetPassword(string Password);
        abstract protected void SetCallback(UnRarCallback callback, IntPtr UserData);
        #endregion

        protected IntPtr handle;

        public static void TryResult(int result)
        {
            TryResult((RarStatus)Enum.ToObject(typeof(RarStatus), result));
        }
        public static void TryResult(RarStatus result)
        {
            switch (result)
            {
                case RarStatus.BAD_ARCHIVE:
                case RarStatus.BAD_DATA:
                case RarStatus.ECLOSE:
                case RarStatus.ECREATE:
                case RarStatus.EOPEN:
                case RarStatus.EREAD:
                case RarStatus.EWRITE:
                case RarStatus.NO_MEMORY:
                case RarStatus.SMALL_BUF:
                case RarStatus.UNKNOWN_FORMAT:
                    throw new RarException(result);
            }
        }

        private RAROpenArchiveDataEx od;
        public RarArchive(string FileName, RarOpenMode Mode, UnRarCallback callback)
        {
            od = new RAROpenArchiveDataEx();
            od.ArcName = FileName;
            od.OpenMode = Mode;
            handle = OpenArchive(ref od);
            if (handle == IntPtr.Zero || od.OpenResult != RarStatus.SUCCESS)
            {
                throw new RarException(RarStatus.EOPEN);
            }
            if (callback != null)
            {
                SetCallback(callback, IntPtr.Zero);
            }
        }

        public void Close()
        {
            if (handle != IntPtr.Zero)
            {
                CloseArchive();
                handle = IntPtr.Zero;
            }
        }
        public void Dispose()
        {
            Close();
        }

        static bool isX64 = IntPtr.Size == 8;
        static bool hasSSE3 = false;

        [DllImportAttribute("CPUFeatures_Win32.dll", EntryPoint = "hasSSE3", CallingConvention = CallingConvention.StdCall)]
        private static extern uint HasSSE3_32();
        [DllImportAttribute("CPUFeatures_x64.dll", EntryPoint = "hasSSE3", CallingConvention = CallingConvention.StdCall)]
        private static extern uint HasSSE3_64();

        static RarArchive()
        {
            if (isX64)
            {
                hasSSE3 = HasSSE3_64() != 0;
            }
            else
            {
                hasSSE3 = HasSSE3_32() != 0;
            }
        }


        public static RarArchive Open(string FileName, RarOpenMode Mode, UnRarCallback callback)
        {
            if (isX64)
            {
                if (hasSSE3)
                {
                    return new RarArchiveX64SSE4(FileName, Mode, callback);
                }
                return new RarArchiveX64Release(FileName, Mode, callback);
            }
            if (hasSSE3)
            {
                return new RarArchiveWin32SSE4(FileName, Mode, callback);
            }
            return new RarArchiveWin32Release(FileName, Mode, callback);
        }
    }
}
