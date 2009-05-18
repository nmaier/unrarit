using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace UnRarIt
{
    class ZipItemInfo : IArchiveEntry
    {
        private ZipEntry entry;
        FileInfo dest = null;

        public ulong CompressedSize
        {
            get { return (ulong)entry.CompressedSize; }
        }

        public uint Crc
        {
            get { return (uint)entry.Crc; }
        }

        public DateTime DateTime
        {
            get { return entry.DateTime; }
        }

        public System.IO.FileInfo Destination
        {
            get { return dest; }
            set { dest = value; }
        }

        public bool IsCrypted
        {
            get { return entry.IsCrypted; }
        }

        public string Name
        {
            get { return entry.Name; }
        }

        public ulong Size
        {
            get { return (ulong)entry.Size; }
        }

        public uint Version
        {
            get { return (uint)entry.Version; }
        }

        public ZipItemInfo(ZipEntry aEntry)
        {
            entry = aEntry;
        }
    }
    class ZipArchiveFile : IArchiveFile
    {
        private FileInfo archive;
        private string password = string.Empty;

        Dictionary<string, IArchiveEntry> items = new Dictionary<string, IArchiveEntry>();

        public ZipArchiveFile(string aArchive)
        {
            archive = new FileInfo(aArchive);
            if (!archive.Exists)
            {
                throw new FileNotFoundException("Zip file not found!", aArchive);
            }
        }

        public FileInfo Archive
        {
            get { return archive; }
        }

        public void Close()
        {
        }

        public void Extract()
        {
            using (ZipFile ar = new ZipFile(archive.FullName))
            {
                byte[] data = new byte[ushort.MaxValue];
                if (!string.IsNullOrEmpty(password))
                {
                    ar.Password = password;
                }
                foreach (ZipEntry e in ar)
                {
                    if (!items.ContainsKey(e.Name))
                    {
                        continue;
                    }
                    IArchiveEntry info = items[e.Name];
                    if (info.Destination == null)
                    {
                        continue;
                    }
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
                    using (Stream istream = ar.GetInputStream(e))
                    {
                        using (FileStream ostream = new FileStream(info.Destination.FullName, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            for (; ; )
                            {
                                int read = istream.Read(data, 0, ushort.MaxValue);
                                if (read <= 0)
                                {
                                    break;
                                }
                                ostream.Write(data, 0, read);
                            }
                        }
                    }
                }


            }
        }

        public event ExtractFileHandler ExtractFile;

        public int ItemCount
        {
            get { return items.Count; }
        }

        public void Open(IEnumerator<string> aPasswords)
        {
            try
            {
                using (ZipFile ar = new ZipFile(archive.FullName))
                {
                    foreach (ZipEntry e in ar)
                    {
                        if (!e.IsFile)
                        {
                            continue;
                        }
                        items.Add(e.Name, new ZipItemInfo(e));
                        if (e.IsCrypted && string.IsNullOrEmpty(password))
                        {
                            bool found = false;
                            aPasswords.MoveNext();
                            for (password = aPasswords.Current; aPasswords.MoveNext(); password = aPasswords.Current)
                            {
                                PasswordEventArgs args = new PasswordEventArgs(password);
                                if (PasswordAttempt != null)
                                {
                                    PasswordAttempt(this, args);
                                }
                                if (!args.ContinueOperation)
                                {
                                    password = string.Empty;
                                    break;
                                }
                                ar.Password = password;
                                try
                                {
                                    using (Stream s = ar.GetInputStream(e))
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                catch (ZipPasswordException) { }
                            }
                            if (!found)
                            {
                                if (PasswordRequired != null)
                                {
                                    for (; ; )
                                    {
                                        PasswordEventArgs args = new PasswordEventArgs();
                                        PasswordRequired(this, args);
                                        if (!args.ContinueOperation || string.IsNullOrEmpty(args.Password))
                                        {
                                            break;
                                        }
                                        try
                                        {
                                            using (Stream s = ar.GetInputStream(e))
                                            {
                                                found = true;
                                                break;
                                            }
                                        }
                                        catch (ZipPasswordException) { }
                                    }
                                    if (!found)
                                    {
                                        throw new ZipException("Password not found!");
                                    }
                                }
                            }
                        }
                    }

                }
            }
            finally
            {
                Close();
            }
        }

        public string Password
        {
            get { return password; }
        }

        public event PasswordAttemptHandler PasswordAttempt;

        public event PasswordRequiredHandler PasswordRequired;

        public void Dispose()
        {
            Close();
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
