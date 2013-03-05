using System;

namespace UnRarIt.Updater
{
  public class UpdateEventArgs : EventArgs
  {
    public Uri InfoUri { get; private set; }
    public Version NewVersion { get; private set; }


    internal UpdateEventArgs(Version aNewVersion, Uri aInfoURI)
    {
      NewVersion = aNewVersion;
      InfoUri = aInfoURI;
    }
  }
}
