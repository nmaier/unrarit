using System;

namespace UnRarIt.Archive
{
  public class PasswordEventArgs : EventArgs
  {
    public bool ContinueOperation { get; set; }
    public string Password { get; set; }


    internal PasswordEventArgs()
      : this(string.Empty)
    {
    }
    internal PasswordEventArgs(string aPassword)
    {
      ContinueOperation = true;
      Password = aPassword;
    }
  }
}
