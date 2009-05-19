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
        public enum RarOpenMode : uint
        {
            LIST = 0,
            EXTRACT = 1,
            LIST_INCSPLIT = 2,
        }

        public enum RarOperation : uint
        {
            SKIP = 0,
            TEST = 1,
            EXTRACT = 2
        }

        public enum RarMessage : uint
        {
            CHANGEVOLUME = 0,
            PROCESSDATA = 1,
            NEEDPASSWORD = 2
        }

        public enum RarVolumeMsg : uint
        {
            ASK = 0,
            NOTIFY = 1
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct RAROpenArchiveDataEx
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string dummy1;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string ArcName;
            public RarOpenMode OpenMode;
            public RarErrors OpenResult;
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

        public delegate int UnRarCallback(RarMessage msg, int UserData, IntPtr WParam, int LParam);


        abstract protected RarErrors CloseArchive(IntPtr hArcData);
        abstract protected IntPtr OpenArchive(ref RAROpenArchiveDataEx ArchiveData);
        abstract protected RarErrors GetHeader(IntPtr hArcData, ref Header HeaderData);
        abstract protected RarErrors ProcessFile(IntPtr hArcData, RarOperation Operation, string DestPath, string DestName);
        abstract public RarErrors GetVersion();
        abstract protected RarErrors SetPassword(IntPtr hArcData, string Password);
        abstract protected void SetCallback(IntPtr hArcData, UnRarCallback callback, int UserData);
        #endregion

        IntPtr handle;

        public static void TryResult(int result)
        {
            TryResult((RarErrors)Enum.ToObject(typeof(RarErrors), result));
        }
        public static void TryResult(RarErrors result)
        {
            switch (result)
            {
                case RarErrors.BAD_ARCHIVE:
                case RarErrors.BAD_DATA:
                case RarErrors.ECLOSE:
                case RarErrors.ECREATE:
                case RarErrors.EOPEN:
                case RarErrors.EREAD:
                case RarErrors.EWRITE:
                case RarErrors.NO_MEMORY:
                case RarErrors.SMALL_BUF:
                case RarErrors.UNKNOWN_FORMAT:
                    throw new RarException(result);
            }
        }

        public RarArchive(string FileName, RarOpenMode Mode, UnRarCallback callback)
        {
            RAROpenArchiveDataEx od = new RAROpenArchiveDataEx();
            od.ArcName = FileName;
            od.OpenMode = Mode;
            handle = OpenArchive(ref od);
            if (handle == IntPtr.Zero || od.OpenResult != RarErrors.SUCCESS)
            {
                throw new RarException(RarErrors.EOPEN);
            }
            if (callback != null)
            {
                SetCallback(handle, callback, 0);
            }
        }

        public void Close()
        {
            if (handle != IntPtr.Zero)
            {
                CloseArchive(handle);
                handle = IntPtr.Zero;
            }
        }
        public void Dispose()
        {
            Close();
        }

        public RarErrors ReadHeader(ref Header header)
        {
            return GetHeader(handle, ref header);
        }
        public RarErrors ProcessFile(RarOperation Operation, string DestPath, string DestName)
        {
            return ProcessFile(handle, Operation, DestPath, DestName);
        }
        public RarErrors SetPassword(string Password)
        {
            return SetPassword(handle, Password);
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
