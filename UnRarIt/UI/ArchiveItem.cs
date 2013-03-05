using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UnRarIt.Interop;
using UnRarIt.Utilities;

namespace UnRarIt
{
  internal class ArchiveItem : ListViewItem, IEnumerable<FileInfo>
  {
    private DirectoryInfo baseDirectory;

    private FileInfo file;

    private readonly Guid format;

    private readonly bool nested;


    internal static ImageList Icons = new ImageList();
    private readonly static Properties.Settings Config = Properties.Settings.Default;
    private Dictionary<string, FileInfo> parts = new Dictionary<string, FileInfo>();
    private readonly static Regex renamer = new Regex("^unrarit_", RegexOptions.Compiled);


    public ArchiveItem(string aFileName, Guid aFormat, bool aNested)
    {
      format = aFormat;
      nested = aNested;

      SubItems.Add(string.Empty);
      SubItems.Add("Ready...");
      file = new FileInfo(aFileName);
      parts.Add(aFileName, file);
      if (!Icons.Images.ContainsKey(file.Extension)) {
        Icons.Images.Add(file.Extension, FileIcon.GetIcon(file.FullName, FileIconSize.Small));
      }
      ImageKey = file.Extension;
      StateImageIndex = 0;

      Invalidate();
    }

    internal Guid Format
    {
      get
      {
        return format;
      }
    }


    public DirectoryInfo BaseDirectory
    {
      get
      {
        if (baseDirectory == null || !baseDirectory.Exists) {
          return file.Directory;
        }
        return baseDirectory;
      }
      set
      {
        baseDirectory = value;
      }
    }
    public FileInfo File
    {
      get
      {
        return file;
      }
    }
    public string FileName
    {
      get
      {
        return file.FullName;
      }
    }
    private string FileSize
    {
      set
      {
        SubItems[1].Text = value;
      }
    }
    public bool IsNested
    {
      get
      {
        return nested;
      }
    }
    public string Status
    {
      set
      {
        SubItems[2].Text = value;
      }
    }
    public string SubStatus
    {
      set
      {
        if (StateImageIndex != 3) {
          return;
        }
        SubItems[2].Text = value;
      }
    }


    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return parts.Values.GetEnumerator();
    }


    protected void Invalidate()
    {
      Text = file.Name + (parts.Count > 1 ? String.Format(
        CultureInfo.CurrentCulture,
        " +{0} parts",
        parts.Count - 1
        ) : string.Empty);

      if (file.Exists) {
        ulong size = 0;
        foreach (FileInfo part in parts.Values) {
          if (part.Exists) {
            size += (ulong)part.Length;
          }
        }
        FileSize = size.ToFormattedSize();
      }
      else {
        FileSize = "missing";
      }
    }


    internal bool AddPart(FileInfo part)
    {
      if (!part.Exists) {
        return false;
      }
      if (!parts.ContainsKey(part.FullName.ToUpperInvariant())) {
        parts[part.FullName.ToUpperInvariant()] = part;
        Invalidate();
        return true;
      }
      return false;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    internal void DeleteFiles()
    {
      try {
        if (file.Exists) {
          file.Delete();
        }
      }
      catch (Exception) {
      }
      foreach (FileInfo part in parts.Values) {
        try {
          if (part.Exists) {
            part.Delete();
          }
        }
        catch (Exception) {
        }
      }
      parts.Clear();
      Remove();
    }

    internal void ExcuteSuccessAction()
    {
      if (IsNested) {
        DeleteFiles();
      }
      else {
        switch (Config.SuccessAction) {
          case 1:
            var oldParts = parts;
            parts = new Dictionary<string, FileInfo>();
            foreach (FileInfo part in oldParts.Values) {
              var newPart = Rename(part);
              parts[newPart.FullName.ToUpperInvariant()] = newPart;
            }
            break;
          case 2:
            DeleteFiles();
            break;
          default:
            break;
        }
      }
      Invalidate();
    }

    internal static FileInfo Rename(FileInfo aFile)
    {
      if (renamer.IsMatch(aFile.Name)) {
        return aFile;
      }
      aFile.MoveTo(Replacements.CombinePath(aFile.Directory.FullName, String.Format(CultureInfo.InvariantCulture, "unrarit_{0}", aFile.Name)));
      return aFile;
    }


    public IEnumerator<FileInfo> GetEnumerator()
    {
      return parts.Values.GetEnumerator();
    }
  }
}
