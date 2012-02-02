using System;
using System.Windows.Forms;

namespace UnRarIt
{
    public partial class SettingsForm : Form
    {
        private static Properties.Settings Config = Properties.Settings.Default;

        public SettingsForm()
        {
            InitializeComponent();
            switch ((OverwriteAction)Config.OverwriteAction)
            {
                default:
                    Ask.Checked = true;
                    break;
                case OverwriteAction.Overwrite:
                    Overwrite.Checked = true;
                    break;
                case OverwriteAction.Skip:
                    Skip.Checked = true;
                    break;
                case OverwriteAction.Rename:
                    Rename.Checked = true;
                    break;
                case OverwriteAction.RenameDirectory:
                    RenameDirectory.Checked = true;
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
            Threads.Value = Config.Threads;
            Nesting.Checked = Config.Nesting;
            LowPriority.Checked = Config.Priority == Interop.ThreadIOPriority.LOW;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Config.Threads = (int)Threads.Value;
            Config.OwnDirectoryLimit = (uint)OwnDirectoryLimit.Value;
            if (Skip.Checked)
            {
                Config.OverwriteAction = (uint)OverwriteAction.Skip;
            }
            else if (Overwrite.Checked)
            {
                Config.OverwriteAction = (uint)OverwriteAction.Overwrite;
            }
            else if (Rename.Checked)
            {
                Config.OverwriteAction = (uint)OverwriteAction.Rename;
            }
            else if (RenameDirectory.Checked)
            {
                Config.OverwriteAction = (uint)OverwriteAction.RenameDirectory;
            }
            else
            {
                Config.OverwriteAction = (uint)OverwriteAction.Unspecified;
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
            Config.Nesting = Nesting.Checked;
            Config.Priority = LowPriority.Checked ? Interop.ThreadIOPriority.LOW : Interop.ThreadIOPriority.NORMAL;

            Config.Save();
            Close();
        }
    }
}
