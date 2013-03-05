using System;
using System.Collections.Generic;

[assembly:CLSCompliant(true)]
namespace UnRarIt.Archive
{
  public interface IArchiveFile : IDisposable, IEnumerable<IArchiveEntry>
  {
    System.IO.FileInfo Archive { get; }
    int ItemCount { get; }
    string Password { get; }

    void Extract();
    void Open(IEnumerator<string> passwords);

    event EventHandler<ExtractFileEventArgs> ExtractFile;
    event EventHandler<ExtractProgressEventArgs> ExtractProgress;
    event EventHandler<PasswordEventArgs> PasswordAttempt;
    event EventHandler<PasswordEventArgs> PasswordRequired;

  }
}
