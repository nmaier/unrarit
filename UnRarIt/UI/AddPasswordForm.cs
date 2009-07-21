using System;
using System.Windows.Forms;

namespace UnRarIt
{
    public partial class AddPasswordForm : Form
    {
        public AddPasswordForm()
        {
            InitializeComponent();

            string cb = Clipboard.GetText().Trim();
            if (!string.IsNullOrEmpty(cb))
            {
                Password.Text = cb;
                Password.SelectAll();
            }
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
