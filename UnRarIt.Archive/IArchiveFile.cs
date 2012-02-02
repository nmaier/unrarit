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
        public enum ExtractionStage
        {
            Extracting,
            Done
        }

        public FileInfo Archive;
        public IArchiveEntry Item;
        public ExtractionStage Stage;

        public bool ContinueOperation = true;

        internal ExtractFileEventArgs(FileInfo aArchive, IArchiveEntry aItem, ExtractionStage aStage)
        {
            Archive = aArchive;
            Item = aItem;
            Stage = aStage;
        }
    }
    public class ExtractProgressEventArgs
    {
        public FileInfo File;
        public long Written;
        public long Total;

        public bool ContinueOperation = true;

        internal ExtractProgressEventArgs(FileInfo aFile, long aWritten, long aTotal)
        {
            File = aFile;
            Written = aWritten;
            Total = aTotal;
        }
    }

    public delegate void ExtractFileHandler(object sender, ExtractFileEventArgs e);
    public delegate void ExtractProgressHandler(object sender, ExtractProgressEventArgs e);
    public delegate void PasswordAttemptHandler(object sender, PasswordEventArgs e);
    public delegate void PasswordRequiredHandler(object sender, PasswordEventArgs e);

    public interface IArchiveFile : IDisposable, IEnumerable<IArchiveEntry>
    {
        System.IO.FileInfo Archive { get; }
        void Extract();
        event ExtractFileHandler ExtractFile;
        event ExtractProgressHandler ExtractProgress;
        int ItemCount { get; }
        void Open(IEnumerator<string> aPasswords);
        string Password { get; }
        event PasswordAttemptHandler PasswordAttempt;
        event PasswordRequiredHandler PasswordRequired;
    }
}
