namespace UnRarIt
{
    partial class OverwriteForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.FileIcon = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ExistingFile = new System.Windows.Forms.Label();
            this.ExistingSize = new System.Windows.Forms.Label();
            this.NewFile = new System.Windows.Forms.Label();
            this.NewSize = new System.Windows.Forms.Label();
            this.OK = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.Overwrite = new System.Windows.Forms.RadioButton();
            this.Skip = new System.Windows.Forms.RadioButton();
            this.Rename = new System.Windows.Forms.RadioButton();
            this.All = new System.Windows.Forms.Button();
            this.Archive = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FileIcon)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.45946F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80.54054F));
            this.tableLayoutPanel1.Controls.Add(this.label2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.FileIcon, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.ExistingFile, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.ExistingSize, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.NewSize, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.NewFile, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.label3, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.label5, 1, 5);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(519, 182);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // FileIcon
            // 
            this.FileIcon.Location = new System.Drawing.Point(3, 3);
            this.FileIcon.Name = "FileIcon";
            this.tableLayoutPanel1.SetRowSpan(this.FileIcon, 5);
            this.FileIcon.Size = new System.Drawing.Size(48, 48);
            this.FileIcon.TabIndex = 0;
            this.FileIcon.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(71, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Existing File:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(71, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "File size:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(71, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "New File:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(71, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "File Size:";
            // 
            // ExistingFile
            // 
            this.ExistingFile.AutoSize = true;
            this.ExistingFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExistingFile.Location = new System.Drawing.Point(158, 0);
            this.ExistingFile.Name = "ExistingFile";
            this.ExistingFile.Size = new System.Drawing.Size(41, 13);
            this.ExistingFile.TabIndex = 5;
            this.ExistingFile.Text = "label5";
            // 
            // ExistingSize
            // 
            this.ExistingSize.AutoSize = true;
            this.ExistingSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExistingSize.Location = new System.Drawing.Point(158, 20);
            this.ExistingSize.Name = "ExistingSize";
            this.ExistingSize.Size = new System.Drawing.Size(41, 13);
            this.ExistingSize.TabIndex = 6;
            this.ExistingSize.Text = "label6";
            // 
            // NewFile
            // 
            this.NewFile.AutoSize = true;
            this.NewFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewFile.Location = new System.Drawing.Point(158, 60);
            this.NewFile.Name = "NewFile";
            this.NewFile.Size = new System.Drawing.Size(41, 13);
            this.NewFile.TabIndex = 7;
            this.NewFile.Text = "label7";
            // 
            // NewSize
            // 
            this.NewSize.AutoSize = true;
            this.NewSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewSize.Location = new System.Drawing.Point(158, 80);
            this.NewSize.Name = "NewSize";
            this.NewSize.Size = new System.Drawing.Size(41, 13);
            this.NewSize.TabIndex = 8;
            this.NewSize.Text = "label8";
            // 
            // OK
            // 
            this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK.Location = new System.Drawing.Point(431, 200);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(100, 23);
            this.OK.TabIndex = 0;
            this.OK.Text = "&Current File";
            this.OK.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28.13239F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 71.86761F));
            this.tableLayoutPanel2.Controls.Add(this.Overwrite, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.Skip, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.Rename, 0, 1);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(158, 125);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 47.76119F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 52.23881F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(358, 54);
            this.tableLayoutPanel2.TabIndex = 9;
            // 
            // Overwrite
            // 
            this.Overwrite.AutoSize = true;
            this.Overwrite.Location = new System.Drawing.Point(3, 3);
            this.Overwrite.Name = "Overwrite";
            this.Overwrite.Size = new System.Drawing.Size(70, 17);
            this.Overwrite.TabIndex = 4;
            this.Overwrite.Text = "Overwrite";
            this.Overwrite.UseVisualStyleBackColor = true;
            // 
            // Skip
            // 
            this.Skip.AutoSize = true;
            this.Skip.Checked = true;
            this.Skip.Location = new System.Drawing.Point(103, 3);
            this.Skip.Name = "Skip";
            this.Skip.Size = new System.Drawing.Size(46, 17);
            this.Skip.TabIndex = 5;
            this.Skip.Text = "Skip";
            this.Skip.UseVisualStyleBackColor = true;
            // 
            // Rename
            // 
            this.Rename.AutoSize = true;
            this.Rename.Location = new System.Drawing.Point(3, 28);
            this.Rename.Name = "Rename";
            this.Rename.Size = new System.Drawing.Size(65, 17);
            this.Rename.TabIndex = 6;
            this.Rename.Text = "Rename";
            this.Rename.UseVisualStyleBackColor = true;
            // 
            // All
            // 
            this.All.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.All.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.All.Location = new System.Drawing.Point(325, 200);
            this.All.Name = "All";
            this.All.Size = new System.Drawing.Size(100, 23);
            this.All.TabIndex = 3;
            this.All.Text = "Use for &all";
            this.All.UseVisualStyleBackColor = true;
            // 
            // Archive
            // 
            this.Archive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Archive.DialogResult = System.Windows.Forms.DialogResult.Retry;
            this.Archive.Location = new System.Drawing.Point(219, 200);
            this.Archive.Name = "Archive";
            this.Archive.Size = new System.Drawing.Size(100, 23);
            this.Archive.TabIndex = 2;
            this.Archive.Text = "&From now on";
            this.Archive.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(71, 122);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Action:";
            // 
            // OverwriteForm
            // 
            this.AcceptButton = this.OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(543, 235);
            this.ControlBox = false;
            this.Controls.Add(this.Archive);
            this.Controls.Add(this.All);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OverwriteForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "A file exists...";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FileIcon)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox FileIcon;
        private System.Windows.Forms.Label ExistingFile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ExistingSize;
        private System.Windows.Forms.Label NewSize;
        private System.Windows.Forms.Label NewFile;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.RadioButton Overwrite;
        private System.Windows.Forms.RadioButton Skip;
        private System.Windows.Forms.RadioButton Rename;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button All;
        private System.Windows.Forms.Button Archive;
        private System.Windows.Forms.Label label5;
    }
}