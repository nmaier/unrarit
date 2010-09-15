using System;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Generic;
namespace UnRarIt.Properties {
    
    
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    internal sealed partial class Settings {
        
        public Settings() {
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
            try
            {
                if (MustUpgrade)
                {
                    Upgrade();
                    MustUpgrade = false;
                    Save();
                }
            }
            catch (Exception)
            {
            }
            if (Threads == 0)
            {
                Threads = Math.Min(Environment.ProcessorCount, 3);
                Save();
            }
        }
        
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Add code to handle the SettingChangingEvent event here.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Add code to handle the SettingsSaving event here.
        }

        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValue("")]
        public List<string> Destinations
        {
            get { return this["Destinations"] as List<string>; }
            set { this["Destinations"] = value; }
        }

    }
}
