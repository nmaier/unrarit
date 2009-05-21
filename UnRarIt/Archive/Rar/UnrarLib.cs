using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using UnRarIt.Interop;
using System.Windows.Forms;

namespace UnRarIt.Archive.Rar
{
    public class RarArchiveFile : IArchiveFile
    {
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

        RarArchive.Header header = new RarArchive.Header();
        RarArchive.UnRarCallback callback;
        bool brokeFile = false;

        public RarArchiveFile(string aFileName)
        {
            archive = new FileInfo(aFileName);
            if (!archive.Exists)
            {
                throw new FileNotFoundException("Archive does not exist", archive.FullName);
            }
        }
        public void Open(IEnumerator<string> aPasswords)
        {
            callback = new RarArchive.UnRarCallback(PasswordCallback);
            passwords = aPasswords;
            RarStatus status;

            for (; ; )
            {
                using (RarArchive ra = RarArchive.Open(archive.FullName, RarOpenMode.LIST, callback))
                {
                    status = ra.GetHeader(ref header);
                    if (status != RarStatus.SUCCESS && status != RarStatus.END_ARCHIVE)
                    {
                        if (string.IsNullOrEmpty(password))
                        {
                            throw new RarException(RarStatus.MISSING_PASSWORD);
                        }
                        continue;
                    }
                    for (; status != RarStatus.END_ARCHIVE; status = ra.GetHeader(ref header))
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
                        RarArchive.TryResult(ra.ProcessFile(RarOperation.SKIP, null, null));
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
                brokeFile = false;
                for (; ; )
                {
                    using (RarArchive ra = RarArchive.Open(archive.FullName, RarOpenMode.EXTRACT, callback))
                    {
                        status = ra.GetHeader(ref header);
                        for (; status != RarStatus.END_ARCHIVE; status = ra.GetHeader(ref header))
                        {
                            RarArchive.TryResult(status);
                            bool rightFile = header.FileName == itemToCrack.Name;
                            status = ra.ProcessFile(rightFile ? RarOperation.TEST : RarOperation.SKIP, null, null);
                            if (status != RarStatus.SUCCESS)
                            {
                                if (!rightFile)
                                {
                                    RarArchive.TryResult(status);
                                }
                                if (string.IsNullOrEmpty(password))
                                {
                                    throw new RarException(RarStatus.MISSING_PASSWORD);
                                }
                                break;
                            }
                            else
                            {
                                if (brokeFile = rightFile)
                                {
                                    break;
                                }
                            }
                        }
                        if (brokeFile)
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
            callback = new RarArchive.UnRarCallback(ExtractCallback);
            RarStatus status;

            using (RarArchive ra = RarArchive.Open(archive.FullName, RarOpenMode.EXTRACT, callback))
            {
                status = ra.GetHeader(ref header);
                for (; status != RarStatus.END_ARCHIVE; status = ra.GetHeader(ref header))
                {
                    RarArchive.TryResult(status);
                    if (!items.ContainsKey(header.FileName))
                    {
                        continue;
                    }
                    RarItemInfo info = items[header.FileName] as RarItemInfo;
                    if (info == null || info.Destination == null)
                    {
                        RarArchive.TryResult(ra.ProcessFile(RarOperation.SKIP, null, null));
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
                        RarArchive.TryResult(ra.ProcessFile(RarOperation.EXTRACT, null, info.Destination.FullName));
                    }
                }
            }

        }

        int PasswordCallback(RarMessage msg, IntPtr UserData, IntPtr WParam, IntPtr LParam)
        {
            switch (msg)
            {
                case RarMessage.CHANGEVOLUME:
                    return HandleVolChange(LParam);

                case RarMessage.NEEDPASSWORD:
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

        int ExtractCallback(RarMessage msg, IntPtr UserData, IntPtr WParam, IntPtr LParam)
        {
            switch (msg)
            {
                case RarMessage.CHANGEVOLUME:
                    return HandleVolChange(LParam);

                case RarMessage.NEEDPASSWORD:
                    if (!string.IsNullOrEmpty(password))
                    {
                        CopyPasswordTo(WParam, LParam, password);
                        return 1;
                    }
                    return -1;
            }
            return 1;
        }

        private static int HandleVolChange(IntPtr LParam)
        {
            if (LParam == IntPtr.Zero)
            {
                // Volume missing, abort
                return -1;
            }
            // Notification, continue
            return 1;
        }

        static void CopyPasswordTo(IntPtr WParam, IntPtr LParam, string Password)
        {
            int length = Math.Min(LParam.ToInt32() - 1, Password.Length);
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
