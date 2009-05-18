using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace UnRarIt
{
    public enum RarErrors : int
    {
        SUCCESS = 0,
        END_ARCHIVE = 10,
        NO_MEMORY = 11,
        BAD_DATA = 12,
        BAD_ARCHIVE = 13,
        UNKNOWN_FORMAT = 14,
        EOPEN = 15,
        ECREATE = 16,
        ECLOSE = 17,
        EREAD = 18,
        EWRITE = 19,
        SMALL_BUF = 20,
        BAD_PASSWORD = 255
    }

    public class RarException : Exception
    {
        RarErrors result;
        public RarErrors Result
        {
            get { return result; }
        }
        public RarException(RarErrors aResult)
            : base(aResult.ToString())
        {
            result = aResult;
        }
    }

    public class RarItemInfo
    {
        string fileName;
        bool isDirectory;
        bool isEncrypted;
        ulong packed;
        ulong unpacked;
        UInt32 crc;
        DateTime fileTime;
        uint version;
        uint method;
        uint attributes;

        public string FileName
        {
            get { return fileName; }
        }
        public bool IsDirectory
        {
            get { return isDirectory; }
        }
        public bool IsEncrypted
        {
            get { return isEncrypted; }
        }
        public ulong UnpackedSize
        {
            get { return unpacked; }
        }
        public ulong PackedSize
        {
            get { return packed; }
        }
        public UInt32 CRC
        {
            get { return crc; }
        }
        public DateTime FileTime
        {
            get { return fileTime; }
        }
        public uint Version
        {
            get { return version; }
        }
        public uint Attributes
        {
            get { return attributes; }
        }

        public RarItemInfo(string aFileName, bool aIsDirectory, bool aIsEncrypted, ulong aPacked, ulong aUnpacked, UInt32 aCRC, DateTime aFileTime, uint aVersion, uint aMethod, uint aAttributes)
        {
            fileName = aFileName;
            isDirectory = aIsDirectory;
            isEncrypted = aIsEncrypted;
            packed = aPacked;
            unpacked = aUnpacked;
            crc = aCRC;
            fileTime = aFileTime;
            version = aVersion;
            method = aMethod;
            attributes = aAttributes;
        }
    }

    public class RarFile : IDisposable, IEnumerable<RarItemInfo>
    {
        const int RAR_OM_LIST = 0;
        const int RAR_OM_EXTRACT = 1;

        const int RAR_SKIP = 0;
        const int RAR_TEST = 1;
        const int RAR_EXTRACT = 2;

        const int RAR_VOL_ASK = 0;
        const int RAR_VOL_NOTIFY = 1;

        enum RarOperations
        {
            EXTRACT = 0,
            TEST = 1,
            LIST = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RAROpenArchiveDataEx
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string ArcName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string ArcNameW;
            public uint OpenMode;
            public int OpenResult;
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
        struct RARHeaderDataEx
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string ArcName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string ArcNameW;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string FileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string FileNameW;

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

        delegate int UnRarCallback(uint msg, int UserData, IntPtr WParam, int LParam);


        [DllImportAttribute("unrar.dll")]
        static extern RarErrors RARCloseArchive(IntPtr hArcData);

        [DllImportAttribute("unrar.dll")]
        static extern IntPtr RAROpenArchiveEx(ref RAROpenArchiveDataEx ArchiveData);

        [DllImportAttribute("unrar.dll")]
        static extern RarErrors RARReadHeaderEx(IntPtr hArcData, ref RARHeaderDataEx HeaderData);

        [DllImportAttribute("unrar.dll", CharSet = CharSet.Unicode)]
        static extern RarErrors RARProcessFileW(IntPtr hArcData, int Operation, string DestPath, string DestName);

        [DllImportAttribute("unrar.dll")]
        static extern RarErrors RARGetDllVersion();

        [DllImportAttribute("unrar.dll", CharSet = CharSet.Ansi)]
        static extern RarErrors RARSetPassword(IntPtr hArcData, string Password);

        [DllImportAttribute("unrar.dll")]
        static extern void RARSetCallback(IntPtr hArcData, UnRarCallback callback, int UserData);

        IntPtr handle;
        bool isSolid = false;
        RAROpenArchiveDataEx openData = new RAROpenArchiveDataEx();
        List<RarItemInfo> items = new List<RarItemInfo>();
        IEnumerator<string> passwords;
        string password = string.Empty;
        string fileName;

        static void TryResult(int result)
        {
            TryResult((RarErrors)Enum.ToObject(typeof(RarErrors), result));
        }
        static void TryResult(RarErrors result)
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



        private static DateTime FromMSDOSTime(uint time)
        {
            int day = 0;
            int month = 0;
            int year = 0;
            int second = 0;
            int hour = 0;
            int minute = 0;
            ushort hiWord;
            ushort loWord;
            hiWord = (ushort)((time & 0xFFFF0000) >> 16);
            loWord = (ushort)(time & 0xFFFF);
            year = ((hiWord & 0xFE00) >> 9) + 1980;
            month = (hiWord & 0x01E0) >> 5;
            day = hiWord & 0x1F;
            hour = (loWord & 0xF800) >> 11;
            minute = (loWord & 0x07E0) >> 5;
            second = (loWord & 0x1F) << 1;
            return new DateTime(year, month, day, hour, minute, second);
        }

        public string Password
        {
            get { return password; }
        }
        public bool IsSolid
        {
            get { return isSolid; }
        }
        public string FileName
        {
            get { return fileName; }
        }

        public RarFile(string aFileName, IEnumerator<string> aPasswords)
        {
            fileName = aFileName;
            passwords = aPasswords;
            RarErrors status;
            openData.ArcName = null;
            openData.ArcNameW = fileName;
            openData.OpenMode = RAR_OM_LIST;
            openData.CmtBuf = new string((char)0, ushort.MaxValue);
            openData.CmtBufSize = ushort.MaxValue;
            UnRarCallback callback = new UnRarCallback(Callback);
            try
            {
                for (; ; )
                {
                    handle = RAROpenArchiveEx(ref openData);
                    TryResult(openData.OpenResult);
                    if (handle == IntPtr.Zero)
                    {
                        throw new RarException(RarErrors.EOPEN);
                    }
                    RARSetCallback(handle, callback, 0);

                    RARHeaderDataEx header = new RARHeaderDataEx();
                    status = RARReadHeaderEx(handle, ref header);
                    isSolid = (openData.Flags & 0x8) == 0x8;

                    if (status != RarErrors.SUCCESS && status != RarErrors.END_ARCHIVE)
                    {
                        if (string.IsNullOrEmpty(password))
                        {
                            throw new RarException(RarErrors.BAD_PASSWORD);
                        }
                        Close();
                        continue;
                    }
                    for (; status != RarErrors.END_ARCHIVE; status = RARReadHeaderEx(handle, ref header))
                    {
                        TryResult(status);
                        if ((header.Flags & 0x1) == 0x1)
                        {
                            // continued from previous
                            continue;
                        }
                        ulong packed = ((ulong)header.PackSizeHigh << 32) + header.PackSize;
                        ulong unpacked = ((ulong)header.UnpSizeHigh << 32) + header.UnpSize;
                        bool isDir = (header.Flags & 0xe0) == 0xe0;
                        bool isEnc = (header.Flags & 0x04) == 0x04;
                        items.Add(new RarItemInfo(header.FileNameW, isDir, isEnc, packed, unpacked, header.FileCRC, FromMSDOSTime(header.FileTime), header.UnpVer, header.Method, header.FileAttr));
                        TryResult(RARProcessFileW(handle, RAR_SKIP, null, null));
                    }
                    break;
                }
            }
            finally
            {
                passwords.Reset();
                Close();
            }
            if (!string.IsNullOrEmpty(password))
            {
                return;
            }
            bool stillNeedPassword = false;
            RarItemInfo itemToCrack = null;
            foreach (RarItemInfo info in items)
            {
                if (info.IsEncrypted)
                {
                    stillNeedPassword = true;
                    if (!info.IsDirectory && (itemToCrack == null || itemToCrack.PackedSize > info.PackedSize))
                    {
                        itemToCrack = info;
                    }
                }
            }
            if (!stillNeedPassword || itemToCrack == null)
            {
                return;
            }
            try
            {
                openData.OpenMode = RAR_OM_EXTRACT;
                for (; ; )
                {
                    handle = RAROpenArchiveEx(ref openData);
                    TryResult(openData.OpenResult);
                    if (handle == IntPtr.Zero)
                    {
                        throw new RarException(RarErrors.EOPEN);
                    }
                    RARSetCallback(handle, callback, 0);

                    RARHeaderDataEx header = new RARHeaderDataEx();
                    status = RARReadHeaderEx(handle, ref header);
                    bool ok = false;
                    for (; status != RarErrors.END_ARCHIVE; status = RARReadHeaderEx(handle, ref header))
                    {
                        TryResult(status);
                        bool rightFile = header.FileNameW == itemToCrack.FileName;
                        status = RARProcessFileW(handle, rightFile ? RAR_TEST : RAR_SKIP, null, null);
                        if (status != RarErrors.SUCCESS)
                        {
                            if (!rightFile)
                            {
                                TryResult(status);
                            }
                            if (string.IsNullOrEmpty(password))
                            {
                                throw new RarException(RarErrors.BAD_PASSWORD);
                            }
                        }
                        else
                        {
                            ok = rightFile;
                        }
                    }
                    if (ok)
                    {
                        break;
                    }
                }
            }
            finally
            {
                passwords.Reset();
                Close();
            }
        }

        int Callback(uint msg, int UserData, IntPtr WParam, int LParam)
        {
            switch (msg)
            {
                case 0: // aka. VolumneChange
                    if (LParam == 0)
                    {
                        // Volume missing, abort
                        return -1;
                    }
                    // Notification, continue
                    return 1;

                case 1: // aka. ProcessData
                    // continue
                    return 1;

                case 2: // aka. NeedPassword
                    if (passwords.MoveNext())
                    {
                        password = passwords.Current;
                        int length = Math.Min(LParam - 1, password.Length);
                        for (int i = 0; i < length; ++i)
                        {
                            Marshal.WriteByte(WParam, i, (byte)password[i]);
                        }
                        Marshal.WriteByte(WParam, length, (byte)0);
                        return 1;
                    }
                    Marshal.WriteByte(WParam, 0, (byte)0);
                    password = string.Empty;
                    return 1;
            }
            return 1;
        }


        public void Close()
        {
            if (handle != IntPtr.Zero)
            {
                RARCloseArchive(handle);
                handle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Close();
            passwords.Dispose();
        }

        #region IEnumerable<RarItemInfo> Members

        public IEnumerator<RarItemInfo> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        #endregion
    }
}
