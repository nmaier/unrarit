using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnRarIt.Utilities
{
  internal class PasswordList : IEnumerable<string>, IDisposable
  {
    private bool dirty = false;

    private readonly FileInfo file;

    private readonly List<Password> passwords = new List<Password>();

    private readonly Dictionary<string, Password> used = new Dictionary<string, Password>();


    public PasswordList()
    {
      file = new FileInfo(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".unrarit"), "passwords.lst"));
      if (!file.Directory.Exists) {
        file.Directory.Create();
      }
      Load();
      dirty = false;
    }


    public int Length
    {
      get {
        return passwords.Count;
      }
    }


    private void AddFromStream(StreamReader r)
    {
      string line;
      while ((line = r.ReadLine()) != null) {
        var count = 0u;
        var lastUsed = 0;

        line = line.Trim();
        if (line.Contains("\t")) {
          var pieces = line.Split(new char[] { '\t' });
          if (pieces.Length >= 2) {
            if (!uint.TryParse(pieces[1], out count)) {
              count = 0;
            }
          }
          if (pieces.Length >= 3) {
            if (!int.TryParse(pieces[2], out lastUsed)) {
              lastUsed = 0;
            }
          }
          line = pieces[0];
        }
        if (string.IsNullOrEmpty(line)) {
          continue;
        }
        var toAdd = new Password(line, count, lastUsed);
        var idx = passwords.IndexOf(toAdd);
        if (idx == -1) {
          passwords.Add(toAdd);
        }
        else {
          passwords[idx] = new Password(toAdd.Pass, passwords[idx].Count + toAdd.Count, Math.Max(toAdd.LastUsed, passwords[idx].LastUsed));
        }
      }
      passwords.Sort();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new PassWordEnumerator(used.Keys, passwords);
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
      return new PassWordEnumerator(used.Keys, passwords);
    }

    private void Load()
    {
      passwords.Clear();
      if (!file.Exists) {
        return;
      }
      try {
        using (var r = new StreamReader(file.FullName, Encoding.UTF8)) {
          AddFromStream(r);
        }
      }
      catch (IOException) {
      }
    }

    private void SaveToStream(Stream stream)
    {
      stream.SetLength(0);
      using (var w = new StreamWriter(stream, Encoding.UTF8)) {
        foreach (Password p in passwords) {
          w.WriteLine("{0}\t{1}\t{2}", p.Pass, p.Count, p.LastUsed);
        }
      }
    }


    public void AddFromFile(string aFile)
    {
      using (var r = new StreamReader(aFile, Encoding.UTF8)) {
        AddFromStream(r);
        dirty = true;
        Save();
      }
    }

    public void Clear()
    {
      passwords.Clear();
      dirty = true;
      Save();
    }

    public void Dispose()
    {
      Save();
    }

    public void Save()
    {
      if (!dirty) {
        return;
      }
      foreach (Password p in used.Values) {
        var idx = passwords.IndexOf(p);
        if (idx != -1) {
          passwords[idx].Merge(p);
        }
        else {
          passwords.Add(p);
        }
      }
      passwords.Sort();
      using (Stream stream = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 1 << 19)) {
        SaveToStream(stream);
      }
      dirty = false;
    }

    public void SaveToFile(string aFile)
    {
      using (Stream stream = new FileStream(aFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 1 << 19)) {
        SaveToStream(stream);
      }
    }

    public void SetGood(string password)
    {
      dirty = true;
      if (used.ContainsKey(password)) {
        used[password].Mark();
      }
      else {
        used[password] = new Password(password);
        Save();
      }
    }


    private sealed class Password : IComparable<Password>, IEquatable<Password>
    {
      private readonly static int baseStamp = GetStamp();

      private uint count;

      private int lastUsed;

      private readonly string password;

      private long score;


      public Password(string aPassword)
      {
        password = aPassword;
        count = 0;
        lastUsed = baseStamp;
        setScore();
      }
      public Password(string aPassword, uint aCount, int aLastUsed)
      {
        password = aPassword;
        count = aCount;
        lastUsed = aLastUsed;
        setScore();
      }


      public uint Count
      {
        get {
          return count;
        }
      }
      public int LastUsed
      {
        get {
          return lastUsed;
        }
      }
      public string Pass
      {
        get {
          return password;
        }
      }


      private static int GetStamp()
      {
        return (DateTime.Now.Year * 100) + DateTime.Now.Month;
      }

      private void setScore()
      {
        score = 200 - Math.Min(200, Math.Max(0, (int)baseStamp - lastUsed));
        score += count * score;
      }


      public int CompareTo(Password other)
      {
        if (other == null) {
          throw new ArgumentNullException("other");
        }
        if (score > other.score) {
          return -1;
        }
        if (score < other.score) {
          return 1;
        }
        return StringComparer.InvariantCulture.Compare(password, other.password);
      }

      public bool Equals(Password other)
      {
        if (other == null) {
          throw new ArgumentNullException("other");
        }
        return password == other.password;
      }

      public void Mark()
      {
        count++;
        lastUsed = baseStamp;
      }

      public void Merge(Password pass)
      {
        count += pass.count;
        lastUsed = baseStamp;
      }

      public override string ToString()
      {
        return String.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}:{3}", password, score, count, lastUsed);
      }
    }

    private class PassWordEnumerator : IEnumerator<string>
    {
      private IEnumerator<string> enumerator;

      private List<string> passwords = new List<string>();


      public PassWordEnumerator(IEnumerable<string> aLastGood, IEnumerable<Password> aGeneral)
      {
        passwords.AddRange(aLastGood);
        foreach (Password p in aGeneral) {
          passwords.Add(p.Pass);
        }
        enumerator = passwords.GetEnumerator();
      }


      string IEnumerator<string>.Current
      {
        get {
          return enumerator.Current;
        }
      }


      public object Current
      {
        get {
          return enumerator.Current;
        }
      }


      public void Dispose()
      {
        enumerator.Dispose();
        passwords = null;
        enumerator = null;
      }

      public bool MoveNext()
      {
        return enumerator.MoveNext();
      }

      public void Reset()
      {
        enumerator.Reset();
      }
    }
  }
}
