using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Emmy.Interop;

namespace UnRarIt
{
    public enum OverwriteAction
    {
        Unspecified,
        Overwrite,
        Skip,
        Rename
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
            FileIcon.Image = new FileIconWin().GetIcon(aExisting, ExtractIconSize.ExtraLarge);
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
