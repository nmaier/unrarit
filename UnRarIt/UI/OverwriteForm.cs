using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UnRarIt.Interop;

namespace UnRarIt
{
    public partial class OverwriteForm : Form
    {
        public OverwriteForm(string existing, string existingSize, string newFile, string newSize)
        {
            InitializeComponent();
            ExistingFile.Text = existing;
            ExistingSize.Text = existingSize;
            NewFile.Text = newFile;
            NewSize.Text = newSize;
            FileIcon.Image = UnRarIt.Interop.FileIcon.GetIcon(existing, FileIconSize.ExtraLarge);
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
