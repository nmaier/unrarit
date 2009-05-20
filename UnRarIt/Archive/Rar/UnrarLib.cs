using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using UnRarIt.Interop;

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
            passwords = aPasswords;
            RarErrors status;
            RarArchive.UnRarCallback callback = new RarArchive.UnRarCallback(Callback);

            for (; ; )
            {
                using (RarArchive ra = RarArchive.Open(archive.FullName, RarArchive.RarOpenMode.LIST, callback))
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
                    using (RarArchive ra = RarArchive.Open(archive.FullName, RarArchive.RarOpenMode.EXTRACT, callback))
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
                                if (ok = rightFile)
                                {
                                    break;
                                }
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

            using (RarArchive ra = RarArchive.Open(archive.FullName, RarArchive.RarOpenMode.EXTRACT, callback))
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
