using System;
using System.Threading;
using System.ComponentModel;
using System.Net;

namespace UnRarIt.Updater
{
    public interface UpdateTarget : ISynchronizeInvoke
    {
        Version AppVersion { get; }
        Uri UpdateURI { get; }
    }

    public enum UpdateCheckType
    {
        Periodical,
        Forced
    };

    public delegate void HasUpdate(Version NewVersion, Uri InfoURI);

    public sealed class Updater
    {
        private readonly UpdateTarget target;
        private bool running = false;

        public event HasUpdate OnHasUpdate;

        private void CheckInternal(object aType)
        {
            var type = (UpdateCheckType)aType;
            for (var i = 0; i < 10; ++i)
            {
                try
                {
                    var updateURI = new UriBuilder(target.UpdateURI);
                    updateURI.Query = String.Format(
                        "version={0}&os={1}",
                        target.AppVersion,
                        Environment.OSVersion.Version
                        );

                    var client = new WebClient();
                    var res = client.DownloadString(updateURI.Uri).Split('\n');
                    var cmp = new Version(res[0]);
                    Uri infoURI = new Uri(res[1]);
                    if (cmp > target.AppVersion)
                    {
                        lock (this)
                        {
                            var lastVersion = new Version(Properties.Settings.Default.LastVersion);
                            if (type == UpdateCheckType.Periodical && cmp <= lastVersion)
                            {
                                break;
                            }
                            Properties.Settings.Default.LastVersion = res[0];
                            Properties.Settings.Default.Save();

                            target.Invoke(new Action(delegate()
                            {
                                if (OnHasUpdate != null)
                                    OnHasUpdate(cmp, infoURI);
                            }), null);
                        }
                    }
                }
                catch (WebException)
                {
                    continue;
                }
                break;
            }
            lock (this)
            {
                running = false;
            }
        }

        public Updater(UpdateTarget aTarget)
        {
            target = aTarget;
        }

        public bool Check(UpdateCheckType aType = UpdateCheckType.Periodical)
        {
            try
            {
                lock (this)
                {
                    if (running)
                    {
                        return false;
                    }
                    var lastChecked = new DateTime();
                    try
                    {
                        lastChecked = Properties.Settings.Default.LastChecked;
                    }
                    catch (Exception) { }
                    if (aType == UpdateCheckType.Periodical && DateTime.Now.Subtract(lastChecked).Days < 7)
                    {
                        return false;
                    }
                    Properties.Settings.Default.LastChecked = DateTime.Now;
                    Properties.Settings.Default.Save();

                    running = true;
                }

                var thread = new Thread(CheckInternal);
                thread.IsBackground = true;
                thread.Start(aType);
                return true;
            }
            catch (Exception) { }

            return false;
        }
    }
}
