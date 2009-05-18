namespace UnRarIt
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Ask = new System.Windows.Forms.RadioButton();
            this.Overwrite = new System.Windows.Forms.RadioButton();
            this.Skip = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.OwnDirectoryLimit = new System.Windows.Forms.NumericUpDown();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.SuccessRemove = new System.Windows.Forms.RadioButton();
            this.SuccessRename = new System.Windows.Forms.RadioButton();
            this.SuccesNothing = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.RemoveAll = new System.Windows.Forms.RadioButton();
            this.RemoveDone = new System.Windows.Forms.RadioButton();
            this.RemoveNone = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OwnDirectoryLimit)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(343, 233);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(262, 233);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Ask);
            this.groupBox1.Controls.Add(this.Overwrite);
            this.groupBox1.Controls.Add(this.Skip);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 100);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "When a file exists...";
            // 
            // Ask
            // 
            this.Ask.AutoSize = true;
            this.Ask.Location = new System.Drawing.Point(17, 65);
            this.Ask.Name = "Ask";
            this.Ask.Size = new System.Drawing.Size(43, 17);
            this.Ask.TabIndex = 3;
            this.Ask.TabStop = true;
            this.Ask.Text = "Ask";
            this.Ask.UseVisualStyleBackColor = true;
            // 
            // Overwrite
            // 
            this.Overwrite.AutoSize = true;
            this.Overwrite.Location = new System.Drawing.Point(17, 42);
            this.Overwrite.Name = "Overwrite";
            this.Overwrite.Size = new System.Drawing.Size(70, 17);
            this.Overwrite.TabIndex = 2;
            this.Overwrite.TabStop = true;
            this.Overwrite.Text = "Overwrite";
            this.Overwrite.UseVisualStyleBackColor = true;
            // 
            // Skip
            // 
            this.Skip.AutoSize = true;
            this.Skip.Location = new System.Drawing.Point(17, 19);
            this.Skip.Name = "Skip";
            this.Skip.Size = new System.Drawing.Size(46, 17);
            this.Skip.TabIndex = 1;
            this.Skip.TabStop = true;
            this.Skip.Text = "Skip";
            this.Skip.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.OwnDirectoryLimit);
            this.groupBox2.Location = new System.Drawing.Point(12, 118);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 100);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Directories...";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(14, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 53);
            this.label1.TabIndex = 1;
            this.label1.Text = "Create a directory when more than this files aren\'t contained within a directory";
            // 
            // OwnDirectoryLimit
            // 
            this.OwnDirectoryLimit.Location = new System.Drawing.Point(155, 21);
            this.OwnDirectoryLimit.Name = "OwnDirectoryLimit";
            this.OwnDirectoryLimit.Size = new System.Drawing.Size(39, 20);
            this.OwnDirectoryLimit.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.SuccessRemove);
            this.groupBox3.Controls.Add(this.SuccessRename);
            this.groupBox3.Controls.Add(this.SuccesNothing);
            this.groupBox3.Location = new System.Drawing.Point(218, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(200, 100);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Processed archives will be...";
            // 
            // SuccessRemove
            // 
            this.SuccessRemove.AutoSize = true;
            this.SuccessRemove.Location = new System.Drawing.Point(17, 65);
            this.SuccessRemove.Name = "SuccessRemove";
            this.SuccessRemove.Size = new System.Drawing.Size(60, 17);
            this.SuccessRemove.TabIndex = 3;
            this.SuccessRemove.TabStop = true;
            this.SuccessRemove.Text = "deleted";
            this.SuccessRemove.UseVisualStyleBackColor = true;
            // 
            // SuccessRename
            // 
            this.SuccessRename.AutoSize = true;
            this.SuccessRename.Location = new System.Drawing.Point(17, 42);
            this.SuccessRename.Name = "SuccessRename";
            this.SuccessRename.Size = new System.Drawing.Size(66, 17);
            this.SuccessRename.TabIndex = 2;
            this.SuccessRename.TabStop = true;
            this.SuccessRename.Text = "renamed";
            this.SuccessRename.UseVisualStyleBackColor = true;
            // 
            // SuccesNothing
            // 
            this.SuccesNothing.AutoSize = true;
            this.SuccesNothing.Location = new System.Drawing.Point(17, 19);
            this.SuccesNothing.Name = "SuccesNothing";
            this.SuccesNothing.Size = new System.Drawing.Size(68, 17);
            this.SuccesNothing.TabIndex = 1;
            this.SuccesNothing.TabStop = true;
            this.SuccesNothing.Text = "left alone";
            this.SuccesNothing.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.RemoveAll);
            this.groupBox4.Controls.Add(this.RemoveDone);
            this.groupBox4.Controls.Add(this.RemoveNone);
            this.groupBox4.Location = new System.Drawing.Point(218, 118);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(200, 100);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Remove on completion...";
            // 
            // RemoveAll
            // 
            this.RemoveAll.AutoSize = true;
            this.RemoveAll.Location = new System.Drawing.Point(17, 65);
            this.RemoveAll.Name = "RemoveAll";
            this.RemoveAll.Size = new System.Drawing.Size(64, 17);
            this.RemoveAll.TabIndex = 6;
            this.RemoveAll.TabStop = true;
            this.RemoveAll.Text = "Clear list";
            this.RemoveAll.UseVisualStyleBackColor = true;
            // 
            // RemoveDone
            // 
            this.RemoveDone.AutoSize = true;
            this.RemoveDone.Location = new System.Drawing.Point(17, 42);
            this.RemoveDone.Name = "RemoveDone";
            this.RemoveDone.Size = new System.Drawing.Size(113, 17);
            this.RemoveDone.TabIndex = 5;
            this.RemoveDone.TabStop = true;
            this.RemoveDone.Text = "Extracted archives";
            this.RemoveDone.UseVisualStyleBackColor = true;
            // 
            // RemoveNone
            // 
            this.RemoveNone.AutoSize = true;
            this.RemoveNone.Location = new System.Drawing.Point(17, 19);
            this.RemoveNone.Name = "RemoveNone";
            this.RemoveNone.Size = new System.Drawing.Size(91, 17);
            this.RemoveNone.TabIndex = 4;
            this.RemoveNone.TabStop = true;
            this.RemoveNone.Text = "No items at all";
            this.RemoveNone.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(430, 268);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Preferences";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.OwnDirectoryLimit)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton Ask;
        private System.Windows.Forms.RadioButton Overwrite;
        private System.Windows.Forms.RadioButton Skip;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown OwnDirectoryLimit;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton SuccessRemove;
        private System.Windows.Forms.RadioButton SuccessRename;
        private System.Windows.Forms.RadioButton SuccesNothing;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton RemoveAll;
        private System.Windows.Forms.RadioButton RemoveDone;
        private System.Windows.Forms.RadioButton RemoveNone;
    }
}