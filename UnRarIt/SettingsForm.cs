using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UnRarIt
{
    public partial class SettingsForm : Form
    {
        private static Properties.Settings Config = Properties.Settings.Default;

        public SettingsForm()
        {
            InitializeComponent();
            switch (Config.OverwriteAction)
            {
                default:
                    Skip.Checked = true;
                    break;
                case 1:
                    Overwrite.Checked = true;
                    break;
                case 2:
                    Ask.Checked = true;
                    break;
            }
            switch (Config.SuccessAction)
            {
                default:
                    SuccesNothing.Checked = true;
                    break;
                case 1:
                    SuccessRename.Checked = true;
                    break;
                case 2:
                    SuccessRemove.Checked = true;
                    break;
            }
            switch (Config.EmptyListWhenDone)
            {
                default:
                    RemoveNone.Checked = true;
                    break;
                case 1:
                    RemoveAll.Checked = true;
                    break;
                case 2:
                    RemoveDone.Checked = true;
                    break;
            }
            OwnDirectoryLimit.Value = Config.OwnDirectoryLimit;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Config.OwnDirectoryLimit = (uint)OwnDirectoryLimit.Value;
            if (Skip.Checked)
            {
                Config.OverwriteAction = 0;
            }
            else if (Overwrite.Checked)
            {
                Config.OverwriteAction = 1;
            }
            else
            {
                Config.OverwriteAction = 2;
            }

            if (SuccesNothing.Checked)
            {
                Config.SuccessAction = 0;
            }
            else if (SuccessRename.Checked)
            {
                Config.SuccessAction = 1;
            }
            else
            {
                Config.SuccessAction = 2;
            }

            if (RemoveNone.Checked)
            {
                Config.EmptyListWhenDone = 0;
            }
            else if (RemoveAll.Checked)
            {
                Config.EmptyListWhenDone = 1;
            }
            else
            {
                Config.EmptyListWhenDone = 2;
            }

            Config.Save();
            Close();
        }
    }
}
