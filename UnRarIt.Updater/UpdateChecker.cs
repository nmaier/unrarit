using System;
using System.Globalization;
using System.Net;
using System.Threading;

[assembly: CLSCompliant(true)]
namespace UnRarIt.Updater
{
  public sealed class UpdateChecker
  {
    private bool running = false;

    private readonly IUpdateTarget target;


    public UpdateChecker(IUpdateTarget target)
    {
      this.target = target;
    }


    public event EventHandler<UpdateEventArgs> OnHasUpdate;


    private void CheckInternal(object aType)
    {
      var type = (UpdateCheckType)aType;
      for (var i = 0; i < 10; ++i) {
        try {
          var updateURI = new UriBuilder(target.UpdateUri)
          {
            Query = String.Format(CultureInfo.InvariantCulture, "version={0}&os={1}",
            target.AppVersion, Environment.OSVersion.Version)
          };

          using (var client = new WebClient()) {
            var res = client.DownloadString(updateURI.Uri).Split('\n');
            var cmp = new Version(res[0]);
            var infoURI = new Uri(res[1]);
            if (cmp > target.AppVersion) {
              lock (this) {
                var lastVersion = new Version(Properties.Settings.Default.LastVersion);
                if (type == UpdateCheckType.Periodical && cmp <= lastVersion) {
                  break;
                }
                Properties.Settings.Default.LastVersion = res[0];
                Properties.Settings.Default.Save();

                target.Invoke(new Action(delegate()
                {
                  if (OnHasUpdate != null) {
                    OnHasUpdate(this, new UpdateEventArgs(cmp, infoURI));
                  }
                }), null);
              }
            }
          }
        }
        catch (WebException) {
          continue;
        }
        break;
      }
      lock (this) {
        running = false;
      }
    }

    public bool Check()
    {
      return Check(UpdateCheckType.Periodical);
    }


    public bool Check(UpdateCheckType updateType)
    {
      try {
        lock (this) {
          if (running) {
            return false;
          }
          var lastChecked = new DateTime();
          try {
            lastChecked = Properties.Settings.Default.LastChecked;
          }
          catch (Exception) {
          }
          if (updateType == UpdateCheckType.Periodical && DateTime.Now.Subtract(lastChecked).Days < 7) {
            return false;
          }
          Properties.Settings.Default.LastChecked = DateTime.Now;
          Properties.Settings.Default.Save();

          running = true;
        }

        var thread = new Thread(CheckInternal) { IsBackground = true };
        thread.Start(updateType);
        return true;
      }
      catch (Exception) {
      }

      return false;
    }
  }
}
