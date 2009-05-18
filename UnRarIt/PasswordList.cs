using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.IsolatedStorage;

namespace UnRarIt
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

            public bool Equals(Password other)
            {
                return password == other.password;
            }
        }
        #endregion

        #region PasswordEnumerator
        class PassWordEnumerator : IEnumerator<string>
        {
            int showLastGood = -1;
            string lastGood;
            IEnumerator<Password> enumerator;
            public PassWordEnumerator(string aLastGood, IEnumerator<Password> aEnumerator)
            {
                enumerator = aEnumerator;
                lastGood = aLastGood;
                Reset();
            }
            public bool MoveNext()
            {
                if (++showLastGood == 0)
                {
                    return true;
                }
                return enumerator.MoveNext();
            }
            public void Reset()
            {
                enumerator.Reset();
                showLastGood = !string.IsNullOrEmpty(lastGood) ? -1 : 0;
            }
            public void Dispose()
            {
                enumerator.Dispose();
            }
            public object Current
            {
                get
                {
                    if (showLastGood == 0)
                    {
                        return lastGood;
                    }
                    return enumerator.Current.Pass;
                }
            }
            string IEnumerator<string>.Current
            {
                get
                {
                    if (showLastGood == 0)
                    {

                        return lastGood;
                    }
                    return enumerator.Current.Pass;
                }
            }
        }
        #endregion

        List<Password> passwords = new List<Password>();
        IsolatedStorageFile file;
        string lastGood = String.Empty;
        bool dirty = false;

        public PasswordList()
        {
            file = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            dirty = false;
            try
            {
                using (StreamReader r = new StreamReader(new IsolatedStorageFileStream("passwords", FileMode.Open, FileAccess.Read), Encoding.UTF8))
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
            lastGood = password;
            foreach (Password p in passwords)
            {
                if (p.Pass == lastGood)
                {
                    p.Mark();
                    passwords.Sort();
                    return;
                }
            }
            passwords.Add(new Password(lastGood));
            passwords.Sort();
        }

        public void Save()
        {
            if (!dirty)
            {
                return;
            }
            using (Stream stream = new IsolatedStorageFileStream("passwords", FileMode.OpenOrCreate, FileAccess.Write))
            {
                SaveToStream(stream);
            }
            dirty = false;
        }
        public void SaveToFile(string file)
        {
            using (Stream stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write))
            {
                SaveToStream(stream);
            }
        }

        private void SaveToStream(Stream stream)
        {
            stream.SetLength(0);
            passwords.Sort();
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
            return new PassWordEnumerator(lastGood, passwords.GetEnumerator());
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new PassWordEnumerator(lastGood, passwords.GetEnumerator());
        }


        public void Dispose()
        {
            Save();
            if (file != null)
            {
                file.Dispose();
            }
        }


        public void Clear()
        {
            passwords.Clear();
            dirty = true;
            Save();
        }
    }
}
