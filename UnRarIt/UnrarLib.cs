using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using UnRarIt.Interop;

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
        UNKNOWN = 21,
        MISSING_PASSWORD = 22
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

    #region RarItemInfo
    public class RarItemInfo : UnRarIt.IArchiveEntry
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

        public string Name
        {
            get { return fileName; }
        }
        public bool IsCrypted
        {
            get { return isEncrypted; }
        }
        public ulong Size
        {
            get { return unpacked; }
        }
        public ulong CompressedSize
        {
            get { return packed; }
        }
        public UInt32 Crc
        {
            get { return crc; }
        }
        public DateTime DateTime
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
            fileName = Reimplement.CleanFileName(aFileName);
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
    #endregion

    public class RarFile : UnRarIt.IArchiveFile
    {

        class RarArchive : IDisposable
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
            struct RAROpenArchiveDataEx
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


            [DllImportAttribute("unrar.dll", EntryPoint = "RARCloseArchive", CallingConvention = CallingConvention.StdCall)]
            static extern RarErrors CloseArchive(IntPtr hArcData);

            [DllImportAttribute("unrar.dll", EntryPoint = "RAROpenArchiveEx", CallingConvention = CallingConvention.StdCall)]
            static extern IntPtr OpenArchive(ref RAROpenArchiveDataEx ArchiveData);

            [DllImportAttribute("unrar.dll", EntryPoint = "RARReadHeaderEx", CallingConvention = CallingConvention.StdCall)]
            static extern RarErrors GetHeader(IntPtr hArcData, ref Header HeaderData);

            [DllImportAttribute("unrar.dll", EntryPoint = "RARProcessFileW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
            static extern RarErrors ProcessFile(IntPtr hArcData, RarOperation Operation, string DestPath, string DestName);

            [DllImportAttribute("unrar.dll", EntryPoint = "RARGetDllVersion", CallingConvention = CallingConvention.StdCall)]
            public static extern RarErrors GetVersion();

            [DllImportAttribute("unrar.dll", EntryPoint = "RARSetPassword", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
            static extern RarErrors SetPassword(IntPtr hArcData, string Password);

            [DllImportAttribute("unrar.dll", EntryPoint = "RARSetCallback", CallingConvention = CallingConvention.StdCall)]
            static extern void SetCallback(IntPtr hArcData, UnRarCallback callback, int UserData);
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
        }

        Dictionary<string, IArchiveEntry> items = new Dictionary<string, IArchiveEntry>();
        IEnumerator<string> passwords;
        string password = string.Empty;
        FileInfo archive;

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

        public RarFile(string aFileName)
        {
            archive = new FileInfo(aFileName);
            if (!archive.Exists)
            {
                throw new FileNotFoundException("Archive does not exist", archive.FullName);
            }
        }
        public void Open(IEnumerator<string> aPasswords)
        {
            passwords = aPasswords;
            RarErrors status;
            RarArchive.UnRarCallback callback = new RarArchive.UnRarCallback(Callback);

            for (; ; )
            {
                using (RarArchive ra = new RarArchive(archive.FullName, RarArchive.RarOpenMode.LIST, callback))
                {
                    RarArchive.Header header = new RarArchive.Header();
                    status = ra.ReadHeader(ref header);
                    if (status != RarErrors.SUCCESS && status != RarErrors.END_ARCHIVE)
                    {
                        if (string.IsNullOrEmpty(password))
                        {
                            throw new RarException(RarErrors.MISSING_PASSWORD);
                        }
                        continue;
                    }
                    for (; status != RarErrors.END_ARCHIVE; status = ra.ReadHeader(ref header))
                    {
                        RarArchive.TryResult(status);
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
                        RarArchive.TryResult(ra.ProcessFile(RarArchive.RarOperation.SKIP, null, null));
                    }
                }
                break;
            }

            if (!string.IsNullOrEmpty(password))
            {
                passwords = null;
                return;
            }
            bool stillNeedPassword = false;
            RarItemInfo itemToCrack = null;
            foreach (RarItemInfo info in items.Values)
            {

                if (info.IsCrypted)
                {
                    stillNeedPassword = true;
                    if (itemToCrack == null || itemToCrack.CompressedSize > info.CompressedSize)
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
                for (; ; )
                {
                    using (RarArchive ra = new RarArchive(archive.FullName, RarArchive.RarOpenMode.EXTRACT, callback))
                    {
                        RarArchive.Header header = new RarArchive.Header();
                        status = ra.ReadHeader(ref header);
                        bool ok = false;
                        for (; status != RarErrors.END_ARCHIVE; status = ra.ReadHeader(ref header))
                        {
                            RarArchive.TryResult(status);
                            bool rightFile = header.FileName == itemToCrack.Name;
                            status = ra.ProcessFile(rightFile ? RarArchive.RarOperation.TEST : RarArchive.RarOperation.SKIP, null, null);
                            if (status != RarErrors.SUCCESS)
                            {
                                if (!rightFile)
                                {
                                    RarArchive.TryResult(status);
                                }
                                if (string.IsNullOrEmpty(password))
                                {
                                    throw new RarException(RarErrors.MISSING_PASSWORD);
                                }
                                break;
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
            }
            finally
            {
                passwords = null;
            }
        }
        public void Extract()
        {
            RarErrors status;
            RarArchive.UnRarCallback callback = new RarArchive.UnRarCallback(Callback);

            using (RarArchive ra = new RarArchive(archive.FullName, RarArchive.RarOpenMode.EXTRACT, callback))
            {
                RarArchive.Header header = new RarArchive.Header();
                if (!string.IsNullOrEmpty(password))
                {
                    RarArchive.TryResult(ra.SetPassword(password));
                }
                status = ra.ReadHeader(ref header);
                for (; status != RarErrors.END_ARCHIVE; status = ra.ReadHeader(ref header))
                {
                    RarArchive.TryResult(status);
                    if (!items.ContainsKey(header.FileName))
                    {
                        continue;
                    }
                    RarItemInfo info = items[header.FileName] as RarItemInfo;
                    if (info == null || info.Destination == null)
                    {
                        RarArchive.TryResult(ra.ProcessFile(RarArchive.RarOperation.SKIP, null, null));
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
                        RarArchive.TryResult(ra.ProcessFile(RarArchive.RarOperation.EXTRACT, null, info.Destination.FullName));
                    }
                }
            }

        }

        int Callback(RarArchive.RarMessage msg, int UserData, IntPtr WParam, int LParam)
        {
            switch (msg)
            {
                case RarArchive.RarMessage.CHANGEVOLUME:
                    if (LParam == 0)
                    {
                        // Volume missing, abort
                        return -1;
                    }
                    // Notification, continue
                    return 1;

                case RarArchive.RarMessage.PROCESSDATA:
                    // continue
                    return 1;

                case RarArchive.RarMessage.NEEDPASSWORD:
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

        public void Dispose()
        {
            passwords = null;
        }
        public IEnumerator<IArchiveEntry> GetEnumerator()
        {
            return items.Values.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return items.Values.GetEnumerator();
        }
    }
}
