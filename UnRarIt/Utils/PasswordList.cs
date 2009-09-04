using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace UnRarIt.Utils
{
    internal class PasswordList : IEnumerable<string>, IDisposable
    {
        #region Password
        class Password : IComparable<Password>, IEquatable<Password>
        {
            static uint GetStamp()
            {
                return ((uint)DateTime.Now.Year * 100) + (uint)DateTime.Now.Month;
            }

            string password;
            uint count;
            uint lastUsed;

            public Password(string aPassword)
            {
                password = aPassword;
                count = 0;
                lastUsed = GetStamp();
            }
            public Password(string aPassword, uint aCount, uint aLastUsed)
            {
                password = aPassword;
                count = aCount;
                lastUsed = aLastUsed;
            }
            public int CompareTo(Password rhs)
            {
                if (lastUsed > rhs.lastUsed)
                {
                    return -1;
                }
                else if (lastUsed < rhs.lastUsed)
                {
                    return 1;
                }
                else if (count > rhs.count)
                {
                    return -1;
                }
                else if (count < rhs.count)
                {
                    return 1;
                }
                return password.CompareTo(rhs.password);
            }
            public string Pass
            {
                get { return password; }
            }
            public uint Count
            {
                get { return count; }
            }
            public uint LastUsed
            {
                get { return lastUsed; }
            }
            public void Mark()
            {
                count++;
                lastUsed = GetStamp();
            }
            public void Merge(Password pass)
            {
                count += pass.count;
                lastUsed = GetStamp();
            }

            public bool Equals(Password other)
            {
                return password == other.password;
            }
        }
        #endregion

        #region PasswordEnumerator
        class PassWordEnumerator : IEnumerator<string>
        {
            List<string> passwords = new List<string>();
            IEnumerator<string> enumerator;
            public PassWordEnumerator(IEnumerable<string> aLastGood, IEnumerable<Password> aGeneral)
            {
                // Cannot not use the IEnumerators here.
                // Seems they aren't per instance
                passwords.AddRange(aLastGood);
                foreach (Password p in aGeneral)
                {
                    passwords.Add(p.Pass);
                }
                enumerator = passwords.GetEnumerator();
            }
            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }
            public void Reset()
            {
                enumerator.Reset();
            }
            public void Dispose()
            {
                enumerator.Dispose();
                passwords = null;
                enumerator = null;
            }
            public object Current
            {
                get { return enumerator.Current; }
            }
            string IEnumerator<string>.Current
            {
                get { return enumerator.Current; }
            }
        }
        #endregion

        List<Password> passwords = new List<Password>();
        FileInfo file;
        Dictionary<string, Password> used = new Dictionary<string, Password>();
        bool dirty = false;

        public PasswordList()
        {
            file = new FileInfo(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".unrarit"), "passwords.lst"));
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
            Load();
            dirty = false;
        }

        private void Load()
        {
            passwords.Clear();
            if (!file.Exists)
            {
                return;
            }
            try
            {
                using (StreamReader r = new StreamReader(file.FullName, Encoding.UTF8))
                {
                    AddFromStream(r);
                }
            }
            catch (Exception)
            {
                // no op
            }
        }

        public void AddFromFile(string aFile)
        {
            using (StreamReader r = new StreamReader(aFile, Encoding.UTF8))
            {
                AddFromStream(r);
            }
        }

        private void AddFromStream(StreamReader r)
        {
            string line;
            uint count, lastUsed;
            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                lastUsed = count = 0;
                if (line.Contains("\t"))
                {
                    string[] pieces = line.Split(new char[] { '\t' });
                    if (pieces.Length >= 2)
                    {
                        uint.TryParse(pieces[1], out count);
                    }
                    if (pieces.Length >= 3)
                    {
                        uint.TryParse(pieces[2], out lastUsed);
                    }
                    line = pieces[0];
                }
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                Password toAdd = new Password(line, count, lastUsed);
                int idx = passwords.IndexOf(toAdd);
                if (idx == -1)
                {
                    passwords.Add(toAdd);
                }
                else
                {
                    passwords[idx] = new Password(toAdd.Pass, passwords[idx].Count + toAdd.Count, Math.Max(toAdd.LastUsed, passwords[idx].LastUsed));
                }
                dirty = true;
            }
            passwords.Sort();
        }

        public void SetGood(string password)
        {
            dirty = true;
            if (used.ContainsKey(password))
            {
                used[password].Mark();
            }
            else
            {
                used[password] = new Password(password);
            }
        }

        public void Save()
        {
            if (!dirty)
            {
                return;
            }
            Load();
            foreach (Password p in used.Values)
            {
                int idx = passwords.IndexOf(p);
                if (idx != -1)
                {
                    passwords[idx].Merge(p);
                }
                else
                {
                    passwords.Add(p);
                }
            }
            passwords.Sort();
            using (Stream stream = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                SaveToStream(stream);
            }
            dirty = false;
        }
        public void SaveToFile(string aFile)
        {
            using (Stream stream = new FileStream(aFile, FileMode.OpenOrCreate, FileAccess.Write))
            {
                SaveToStream(stream);
            }
        }

        private void SaveToStream(Stream stream)
        {
            stream.SetLength(0);
            using (StreamWriter w = new StreamWriter(stream, Encoding.UTF8))
            {
                foreach (Password p in passwords)
                {
                    w.WriteLine("{0}\t{1}\t{2}", p.Pass, p.Count, p.LastUsed);
                }
            }
        }

        public int Length
        {
            get { return passwords.Count; }
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return new PassWordEnumerator(used.Keys, passwords);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new PassWordEnumerator(used.Keys, passwords);
        }


        public void Dispose()
        {
            Save();
        }


        public void Clear()
        {
            passwords.Clear();
            dirty = true;
            Save();
        }
    }
}
