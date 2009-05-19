using System;
using System.IO;
using System.Collections.Generic;

namespace UnRarIt.Archive
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
        public IArchiveEntry Item;
        public string Destination;

        public bool ContinueOperation = true;

        internal ExtractFileEventArgs(FileInfo aArchive, IArchiveEntry aItem, string aDestination)
        {
            Archive = aArchive;
            Item = aItem;
            Destination = aDestination;
        }
    }

    public delegate void ExtractFileHandler(object sender, ExtractFileEventArgs e);
    public delegate void PasswordAttemptHandler(object sender, PasswordEventArgs e);
    public delegate void PasswordRequiredHandler(object sender, PasswordEventArgs e);

    public interface IArchiveFile : IDisposable, IEnumerable<IArchiveEntry>
    {
        System.IO.FileInfo Archive { get; }
        void Extract();
        event ExtractFileHandler ExtractFile;
        int ItemCount { get; }
        void Open(IEnumerator<string> aPasswords);
        string Password { get; }
        event PasswordAttemptHandler PasswordAttempt;
        event PasswordRequiredHandler PasswordRequired;
    }
}
