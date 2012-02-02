namespace UnRarIt.Updater.Properties
{
    internal sealed partial class Settings
    {

        public Settings()
        {
            if (MustUpgrade)
            {
                Upgrade();
                MustUpgrade = false;
                Save();
            }
        }

        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            // Add code to handle the SettingChangingEvent event here.
        }

        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Add code to handle the SettingsSaving event here.
        }
    }
}
