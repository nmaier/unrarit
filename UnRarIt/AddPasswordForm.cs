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
    public partial class AddPasswordForm : Form
    {
        public AddPasswordForm()
        {
            InitializeComponent();
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {
            buttonOK.Enabled = !string.IsNullOrEmpty(Password.Text);
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            if (OpenDialog.ShowDialog() == DialogResult.OK)
            {
                Password.Text = OpenDialog.FileName;
                this.DialogResult = DialogResult.Yes;
            }
        }
    }
}
