using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using UnRarIt.Interop;

namespace UnRarIt.Archive
{
  public sealed class ArchiveFile : IArchiveFile, IArchiveOpenCallback, IArchiveOpenVolumeCallback, IProgress, IGetPassword, IDisposable
  {
    private readonly FileInfo archive;

    private FileInfo current;

    private Guid format;

    private bool nextPassword = true;

    private bool passwordDefined = false;

    private bool passwordRequested = false;

    private IEnumerator<string> passwordCollection;

    private readonly ThreadIOPriority priority;


    public static readonly Guid FormatSevenZip = new Guid("23170f69-40c1-278a-1000-000110070000");
    public static readonly Guid FormatZip = new Guid("23170f69-40c1-278a-1000-000110010000");
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rar")]
    public static readonly Guid FormatRar = new Guid("23170f69-40c1-278a-1000-000110030000");
    public static readonly Guid FormatRar5 = new Guid("23170f69-40c1-278a-1000-000110CC0000");
    public static readonly Guid FormatSplit = new Guid("23170f69-40c1-278a-1000-000110EA0000");
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly ReadOnlyCollection<Guid> AllFormats = new ReadOnlyCollection<Guid>(new List<Guid>() { FormatZip, FormatRar, FormatRar5, FormatSevenZip, FormatSplit });
    private readonly Dictionary<string, IArchiveEntry> items = new Dictionary<string, IArchiveEntry>();
    private string currentPassword = string.Empty;
    private readonly Dictionary<string, SevenZipFileStream> fileStreams = new Dictionary<string, SevenZipFileStream>();


    public ArchiveFile(FileInfo archive, Guid format, ThreadIOPriority priority)
    {
      if (archive == null) {
        throw new ArgumentNullException("archive");
      }

      this.archive = archive;
      this.format = format;
      this.priority = priority;

      if (!archive.Exists) {
        throw new FileNotFoundException("Empty List supplied");
      }

      current = archive;
    }


    public event EventHandler<ExtractFileEventArgs> ExtractFile;
    public event EventHandler<ExtractProgressEventArgs> ExtractProgress;
    public event EventHandler<PasswordEventArgs> PasswordAttempt;
    public event EventHandler<PasswordEventArgs> PasswordRequired;


    public FileInfo Archive
    {
      get {
        return archive;
      }
    }
    public int ItemCount
    {
      get {
        return items.Count;
      }
    }
    public string Password
    {
      get {
        return currentPassword;
      }
    }


    private void CloseStreams()
    {
      foreach (SevenZipFileStream s in fileStreams.Values) {
        s.Dispose();
      }
      fileStreams.Clear();
    }

    void IArchiveOpenVolumeCallback.GetProperty(ItemPropId propID, ref PropVariant rv)
    {
      switch (propID) {
        case ItemPropId.Name:
          rv.type = VarEnum.VT_BSTR;
          rv.union.bstrValue = Marshal.StringToBSTR(archive.FullName);
          return;
        case ItemPropId.Size:
          rv.type = VarEnum.VT_UI8;
          rv.union.ui8Value = (ulong)current.Length;
          return;
        default:
          throw new NotImplementedException();
      }
    }

    int IArchiveOpenVolumeCallback.GetStream(string name, ref IInStream stream)
    {
      var c = new FileInfo(name);
      if (!c.Exists) {
        stream = null;
        return 1;
      }
      current = c;
      if (fileStreams.ContainsKey(name)) {
        stream = fileStreams[name];
        stream.Seek(0, 0);
        return 0;
      }
      var fileStream = new SevenZipFileStream(current, FileMode.Open, FileAccess.Read);
      fileStreams[name] = fileStream;
      stream = fileStream;
      return 0;
    }

    public void Dispose()
    {
      CloseStreams();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return items.Values.GetEnumerator();
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
    public void CryptoGetTextPassword(out string password)
    {
      if (passwordDefined || !nextPassword) {
        password = currentPassword;
        return;
      }
      if (passwordCollection == null) {
        throw new IndexOutOfRangeException();
      }
      if (!passwordCollection.MoveNext()) {
        throw new IndexOutOfRangeException();
      }
      passwordRequested = true;
      var args = new PasswordEventArgs(currentPassword);
      if (PasswordAttempt != null) {
        PasswordAttempt(this, args);
      }
      if (!args.ContinueOperation) {
        throw new IndexOutOfRangeException();
      }
      password = currentPassword = passwordCollection.Current;
      nextPassword = false;
    }

    public void Extract()
    {
      try {
        using (var ar = new Archive(archive, this, format)) {
          var indices = new List<uint>();
          var files = new Dictionary<uint, IArchiveEntry>();
          var e = ar.GetNumberOfItems();
          for (uint i = 0; i < e; ++i) {
            var name = ar.GetProperty(i, ItemPropId.Path).GetString();
            if (format == FormatSplit) {
              name = Path.GetFileName(name);
            }
            if (!items.ContainsKey(name)) {
              continue;
            }

            var entry = items[name];
            if (entry.Destination == null) {
              continue;
            }
            indices.Add(i);
            files[i] = entry;
          }
          using (var callback = new ExtractCallback(this, files)) {
            ar.Extract(indices.ToArray(), (uint)indices.Count, ExtractMode.Extract, callback);
          }
        }
      }
      finally {
        CloseStreams();
      }
    }

    public IEnumerator<IArchiveEntry> GetEnumerator()
    {
      return items.Values.GetEnumerator();
    }

    public void Open(IEnumerator<string> passwords)
    {
      if (passwords == null) {
        throw new ArgumentNullException("passwords");
      }
      var opened = false;
      try {
        this.passwordCollection = passwords;
        var formats = new Dictionary<Guid, bool>();
        formats.Add(format, false);
        foreach (Guid f in AllFormats) {
          formats[f] = false;
        }
        foreach (Guid f in formats.Keys) {
          if (opened) {
            break;
          }
          for (; !passwordDefined; ) {
            try {
              using (var ar = new Archive(archive, this, f)) {
                nextPassword = true;
                IArchiveEntry minCrypted = null;
                uint minIndex = 0;
                var e = ar.GetNumberOfItems();
                if (e != 0) {
                  format = f;
                  opened = true;
                  passwordDefined = !string.IsNullOrEmpty(currentPassword);
                }
                else {
                  if (!passwordRequested) {
                    break;
                  }
                }
                for (uint i = 0; i < e; ++i) {
                  if (ar.GetProperty(i, ItemPropId.IsDir).GetBool()) {
                    continue;
                  }
                  var name = ar.GetProperty(i, ItemPropId.Path).GetString();
                  if (format == FormatSplit) {
                    name = Path.GetFileName(name);
                  }

                  var size = ar.GetProperty(i, ItemPropId.Size).GetUlong();
                  var packedSize = ar.GetProperty(i, ItemPropId.PackedSize).GetUlong();
                  if (packedSize == 0) {
                    packedSize = size;
                  }
                  var isCrypted = ar.GetProperty(i, ItemPropId.Encrypted).GetBool();
                  var crc = ar.GetProperty(i, ItemPropId.CRC).GetUint();
                  IArchiveEntry entry = new SevenZipItemInfo(name, crc, isCrypted, size, packedSize);
                  items[name] = entry;
                  if (isCrypted && (minCrypted == null || minCrypted.CompressedSize > packedSize)) {
                    minCrypted = entry;
                    minIndex = i;
                  }
                }
                if (minCrypted != null && !passwordDefined) {
                  passwords.Reset();
                  var files = new Dictionary<uint, IArchiveEntry>();
                  files[minIndex] = minCrypted;
                  for (; !passwordDefined; ) {
                    using (var callback = new ExtractCallback(this, files)) {
                      try {
                        ar.Extract(new uint[] { minIndex }, 1, ExtractMode.Test, callback);
                        passwordDefined = true;
                      }
                      catch (IOException) {
                        nextPassword = true;
                        continue;
                      }
                    }
                  }
                }
                else {
                  if (items.Count != 0) {
                    passwordDefined = true;
                  }
                  else {
                    if (!passwordRequested) {
                      opened = false;
                      break;
                    }
                  }
                }
              }
            }
            catch (IndexOutOfRangeException) {
              throw new ArchiveException("Password missing");
            }
          }
        }
      }
      finally {
        CloseStreams();
      }
      if (!opened) {
        throw new ArchiveException("Invalid archive!");
      }
    }

    [CLSCompliant(false)]
    public void SetCompleted(ref ulong completeValue)
    {
    }

    public void SetCompleted(IntPtr files, IntPtr bytes)
    {
    }

    [CLSCompliant(false)]
    public void SetTotal(ulong total)
    {
    }

    public void SetTotal(IntPtr files, IntPtr bytes)
    {
    }


    private class ExtractCallback : IArchiveExtractCallback, IGetPassword, IDisposable, IProgress, IArchiveOpenVolumeCallback
    {
      private uint current = 0;

      private readonly Dictionary<uint, IArchiveEntry> files;

      private ExtractMode mode = ExtractMode.Skip;

      private readonly ArchiveFile owner;

      private ISevenZipStream stream;


      internal ExtractCallback(ArchiveFile aOwner, Dictionary<uint, IArchiveEntry> aFiles)
      {
        owner = aOwner;
        files = aFiles;
      }


      void IArchiveOpenVolumeCallback.GetProperty(ItemPropId propID, ref PropVariant rv)
      {
        (owner as IArchiveOpenVolumeCallback).GetProperty(propID, ref rv);
      }

      int IArchiveOpenVolumeCallback.GetStream(string name, ref IInStream inStream)
      {
        return (owner as IArchiveOpenVolumeCallback).GetStream(name, ref inStream);
      }


      public void CryptoGetTextPassword(out string aPassword)
      {
        owner.CryptoGetTextPassword(out aPassword);
      }

      public void Dispose()
      {
        if (stream != null) {
          stream.Dispose();
          stream = null;
        }
      }

      public void GetStream(uint index, out ISequentialOutStream outStream, ExtractMode extractMode)
      {
        if (!files.ContainsKey(index)) {
          outStream = null;
          mode = ExtractMode.Skip;
          return;
        }
        if (extractMode == ExtractMode.Extract) {
          if (stream != null) {
            stream.Dispose();
          }
          current = index;
          var args = new ExtractFileEventArgs(owner.Archive, files[current], ExtractionStage.Extracting);
          owner.ExtractFile(owner, args);
          if (!args.ContinueOperation) {
            throw new IOException("User aborted!");
          }
          var ostream = new SevenZipOutFileStream(files[index].Destination, (long)files[index].Size, owner.priority);
          ostream.ProgressHandler += owner.ExtractProgress;
          stream = ostream;
        }
        else {
          if (stream != null) {
            stream.Dispose();
          }
          stream = new SevenZipNullStream();
        }
        outStream = stream;
        mode = extractMode;
      }

      public void PrepareOperation(ExtractMode askExtractMode)
      {
      }

      public void SetCompleted(ref ulong completeValue)
      {
      }

      public void SetOperationResult(OperationResult resultEOperationResult)
      {
        if (mode != ExtractMode.Skip && resultEOperationResult != OperationResult.OK) {
          throw new IOException(resultEOperationResult.ToString());
        }

        if (stream != null) {
          stream.SetOK();
          stream.Dispose();
          stream = null;
        }

        if (mode == ExtractMode.Extract && owner.ExtractFile != null) {
          var args = new ExtractFileEventArgs(owner.Archive, files[current], ExtractionStage.Done);
          owner.ExtractFile(owner, args);
          if (!args.ContinueOperation) {
            throw new IOException("User aborted!");
          }
        }
      }

      public void SetTotal(ulong total)
      {
      }
    }
  }
}
