using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UnRarIt.Interop;

namespace UnRarIt
{
    public enum OverwriteAction
    {
        Unspecified = 2,
        Overwrite = 1,
        Skip = 0,
        Rename = 3,
        RenameDirectory = 4
    };

    public partial class OverwriteForm : Form
    {
        public OverwriteForm(string aExisting, string aExistingSize, string aNew, string aNewSize)
        {
            InitializeComponent();
            ExistingFile.Text = aExisting;
            ExistingSize.Text = aExistingSize;
            NewFile.Text = aNew;
            NewSize.Text = aNewSize;
            FileIcon.Image = new FileIconWin().GetIcon(aExisting, FileIconSize.ExtraLarge);
        }
        public OverwriteAction Action
        {
            get
            {
                if (Overwrite.Checked)
                {
                    return OverwriteAction.Overwrite;
                }
                if (Rename.Checked)
                {
                    return OverwriteAction.Rename;
                }
                return OverwriteAction.Skip;
            }
        }
    }
}
