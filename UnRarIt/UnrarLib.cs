using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace UnRarIt
{
    public class PasswordEventArgs
    {
        public string Password = string.Empty;

        public bool ContinueOperation = true;

        internal PasswordEventArgs()
        {
        }
        internal PasswordEventArgs(string aPassword)
        {
            Password = aPassword;
        }
    }

    public class ExtractFileEventArgs
    {
        public FileInfo Archive;
        public RarItemInfo Item;
        public string Destination;

        public bool ContinueOperation = true;

        internal ExtractFileEventArgs(FileInfo aArchive, RarItemInfo aItem, string aDestination)
        {
            Archive = aArchive;
            Item = aItem;
            Destination = aDestination;
        }
    }

    public delegate void ExtractFileHandler(object sender, ExtractFileEventArgs e);
    public delegate void PasswordAttemptHandler(object sender, PasswordEventArgs e);
    public delegate void PasswordRequiredHandler(object sender, PasswordEventArgs e);

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
        bool isEncrypted;
        ulong packed;
        ulong unpacked;
        UInt32 crc;
        DateTime fileTime;
        uint version;
        uint method;
        uint attributes;


        FileInfo dest;

        public string FileName
        {
            get { return fileName; }
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
        public FileInfo Destination
        {
            get { return dest; }
            set { dest = value; }
        }

        public RarItemInfo(string aFileName, bool aIsEncrypted, ulong aPacked, ulong aUnpacked, UInt32 aCRC, DateTime aFileTime, uint aVersion, uint aMethod, uint aAttributes)
        {
            fileName = aFileName;
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
            public string dummy1;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string ArcName;
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
            public string dummy1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string ArcName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string dummy2;
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

        delegate int UnRarCallback(uint msg, int UserData, IntPtr WParam, int LParam);


        [DllImportAttribute("unrar.dll", EntryPoint = "RARCloseArchive", CallingConvention = CallingConvention.StdCall)]
        static extern RarErrors CloseArchive(IntPtr hArcData);

        [DllImportAttribute("unrar.dll", EntryPoint = "RAROpenArchiveEx", CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr OpenArchive(ref RAROpenArchiveDataEx ArchiveData);

        [DllImportAttribute("unrar.dll", EntryPoint = "RARReadHeaderEx", CallingConvention = CallingConvention.StdCall)]
        static extern RarErrors GetHeader(IntPtr hArcData, ref RARHeaderDataEx HeaderData);

        [DllImportAttribute("unrar.dll", EntryPoint = "RARProcessFileW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        static extern RarErrors ProcessFile(IntPtr hArcData, int Operation, string DestPath, string DestName);

        [DllImportAttribute("unrar.dll", EntryPoint = "RARGetDllVersion", CallingConvention = CallingConvention.StdCall)]
        static extern RarErrors GetVersion();

        [DllImportAttribute("unrar.dll", EntryPoint = "RARSetPassword", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        static extern RarErrors SetPassword(IntPtr hArcData, string Password);

        [DllImportAttribute("unrar.dll", EntryPoint = "RARSetCallback", CallingConvention = CallingConvention.StdCall)]
        static extern void SetCallback(IntPtr hArcData, UnRarCallback callback, int UserData);

        IntPtr handle;
        bool isSolid = false;
        RAROpenArchiveDataEx openData = new RAROpenArchiveDataEx();
        Dictionary<string, RarItemInfo> items = new Dictionary<string, RarItemInfo>();
        IEnumerator<string> passwords;
        string password = string.Empty;
        FileInfo archive;

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
        public FileInfo Archive
        {
            get { return archive; }
        }

        public int ItemCount
        {
            get { return items.Count; }
        }

        public event PasswordRequiredHandler PasswordRequired;
        public event PasswordAttemptHandler PasswordAttempt;
        public event ExtractFileHandler ExtractFile;

        public RarFile(string aFileName, IEnumerator<string> aPasswords)
        {
            archive = new FileInfo(aFileName);
            if (!archive.Exists)
            {
                throw new FileNotFoundException("Archive does not exist", archive.FullName);
            }
            passwords = aPasswords;
        }
        public void Open()
        {
            RarErrors status;
            openData.ArcName = archive.FullName;
            openData.OpenMode = RAR_OM_LIST;
            openData.CmtBuf = new string((char)0, ushort.MaxValue);
            openData.CmtBufSize = ushort.MaxValue;
            UnRarCallback callback = new UnRarCallback(Callback);
            try
            {
                for (; ; )
                {
                    handle = OpenArchive(ref openData);
                    TryResult(openData.OpenResult);
                    if (handle == IntPtr.Zero)
                    {
                        throw new RarException(RarErrors.EOPEN);
                    }
                    SetCallback(handle, callback, 0);

                    RARHeaderDataEx header = new RARHeaderDataEx();
                    status = GetHeader(handle, ref header);
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
                    for (; status != RarErrors.END_ARCHIVE; status = GetHeader(handle, ref header))
                    {
                        TryResult(status);
                        if ((header.Flags & 0x1) == 0x1)
                        {
                            // continued from previous
                            continue;
                        }
                        if ((header.Flags & 0xe0) == 0xe0)
                        {
                            // Directory
                            continue;
                        }

                        ulong packed = ((ulong)header.PackSizeHigh << 32) + header.PackSize;
                        ulong unpacked = ((ulong)header.UnpSizeHigh << 32) + header.UnpSize;
                        bool isEnc = (header.Flags & 0x04) == 0x04;
                        items[header.FileName] = new RarItemInfo(header.FileName, isEnc, packed, unpacked, header.FileCRC, FromMSDOSTime(header.FileTime), header.UnpVer, header.Method, header.FileAttr);
                        TryResult(ProcessFile(handle, RAR_SKIP, null, null));
                    }
                    break;
                }
            }
            finally
            {
                Close();
            }
            if (!string.IsNullOrEmpty(password))
            {
                passwords.Dispose();
                passwords = null;
                return;
            }
            bool stillNeedPassword = false;
            RarItemInfo itemToCrack = null;
            foreach (RarItemInfo info in items.Values)
            {

                if (info.IsEncrypted)
                {
                    stillNeedPassword = true;
                    if (itemToCrack == null || itemToCrack.PackedSize > info.PackedSize)
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
                    handle = OpenArchive(ref openData);
                    TryResult(openData.OpenResult);
                    if (handle == IntPtr.Zero)
                    {
                        throw new RarException(RarErrors.EOPEN);
                    }
                    SetCallback(handle, callback, 0);

                    RARHeaderDataEx header = new RARHeaderDataEx();
                    status = GetHeader(handle, ref header);
                    bool ok = false;
                    for (; status != RarErrors.END_ARCHIVE; status = GetHeader(handle, ref header))
                    {
                        TryResult(status);
                        bool rightFile = header.FileName == itemToCrack.FileName;
                        status = ProcessFile(handle, rightFile ? RAR_TEST : RAR_SKIP, null, null);
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
                passwords.Dispose();
                passwords = null;
                Close();
            }
        }
        public void Extract()
        {
            RarErrors status;
            UnRarCallback callback = new UnRarCallback(Callback);
            try
            {
                RARHeaderDataEx header = new RARHeaderDataEx();
                openData.OpenMode = RAR_OM_EXTRACT;
                handle = OpenArchive(ref openData);
                TryResult(openData.OpenResult);
                if (handle == IntPtr.Zero)
                {
                    throw new RarException(RarErrors.EOPEN);
                }
                if (!string.IsNullOrEmpty(password))
                {
                    SetPassword(handle, password);
                }
                SetCallback(handle, callback, 0);
                status = GetHeader(handle, ref header);
                for (; status != RarErrors.END_ARCHIVE; status = GetHeader(handle, ref header))
                {
                    TryResult(status);
                    if (!items.ContainsKey(header.FileName))
                    {
                        continue;
                    }
                    RarItemInfo info = items[header.FileName];
                    if (info == null || info.Destination == null)
                    {
                        TryResult(ProcessFile(handle, RAR_SKIP, null, null));
                    }
                    else
                    {
                        if (ExtractFile != null)
                        {
                            ExtractFileEventArgs args = new ExtractFileEventArgs(archive, info, info.Destination.FullName);
                            ExtractFile(this, args);
                            if (!args.ContinueOperation)
                            {
                                break;
                            }
                        }

                        if (!info.Destination.Directory.Exists)
                        {
                            info.Destination.Directory.Create();
                        }
                        TryResult(ProcessFile(handle, RAR_EXTRACT, null, info.Destination.FullName));

                    }
                }

            }
            finally
            {
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
                    if (passwords == null)
                    {
                        return -1;
                    }
                    if (passwords.MoveNext())
                    {
                        password = passwords.Current;
                        PasswordEventArgs args = new PasswordEventArgs(password);
                        if (PasswordAttempt != null)
                        {
                            PasswordAttempt(this, args);
                        }
                        if (!args.ContinueOperation)
                        {
                            password = string.Empty;
                        }
                        CopyPasswordTo(WParam, LParam, password);
                        return 1;
                    }
                    PasswordEventArgs req = new PasswordEventArgs();
                    if (PasswordRequired != null)
                    {
                        PasswordRequired(this, req);
                    }

                    if (req.ContinueOperation)
                    {
                        password = req.Password;
                    }
                    else
                    {
                        password = string.Empty;
                    }
                    CopyPasswordTo(WParam, LParam, password);
                    return 1;
            }
            return 1;
        }

        static void CopyPasswordTo(IntPtr WParam, int LParam, string Password)
        {
            int length = Math.Min(LParam - 1, Password.Length);
            for (int i = 0; i < length; ++i)
            {
                Marshal.WriteByte(WParam, i, (byte)Password[i]);
            }
            Marshal.WriteByte(WParam, length, (byte)0);
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
            if (passwords != null)
            {
                passwords.Dispose();
                passwords = null;
            }
        }
        public IEnumerator<RarItemInfo> GetEnumerator()
        {
            return items.Values.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return items.Values.GetEnumerator();
        }
    }
}
