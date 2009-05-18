namespace UnRarIt
{
    partial class Main
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Rar-Archives", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Zip-Archives", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.Files = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.Icons = new System.Windows.Forms.ImageList(this.components);
            this.UnrarIt = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.Progress = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusPasswords = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.homepageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddPassword = new System.Windows.Forms.Button();
            this.BrowseDest = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.BrowseDestDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.Dest = new System.Windows.Forms.TextBox();
            this.States = new System.Windows.Forms.ImageList(this.components);
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Files
            // 
            this.Files.AllowDrop = true;
            this.Files.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.Files.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            listViewGroup3.Header = "Rar-Archives";
            listViewGroup3.Name = "GroupRar";
            listViewGroup4.Header = "Zip-Archives";
            listViewGroup4.Name = "GroupZip";
            this.Files.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup3,
            listViewGroup4});
            this.Files.LargeImageList = this.Icons;
            this.Files.Location = new System.Drawing.Point(12, 27);
            this.Files.Name = "Files";
            this.Files.Size = new System.Drawing.Size(669, 421);
            this.Files.SmallImageList = this.Icons;
            this.Files.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.Files.StateImageList = this.States;
            this.Files.TabIndex = 0;
            this.Files.UseCompatibleStateImageBehavior = false;
            this.Files.View = System.Windows.Forms.View.Details;
            this.Files.SelectedIndexChanged += new System.EventHandler(this.Files_SelectedIndexChanged);
            this.Files.DragDrop += new System.Windows.Forms.DragEventHandler(this.Files_DragDrop);
            this.Files.DragEnter += new System.Windows.Forms.DragEventHandler(this.Files_DragEnter);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "FileName";
            this.columnHeader1.Width = 200;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Size";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader2.Width = 100;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Status";
            this.columnHeader3.Width = 100;
            // 
            // Icons
            // 
            this.Icons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.Icons.ImageSize = new System.Drawing.Size(16, 16);
            this.Icons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // UnrarIt
            // 
            this.UnrarIt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.UnrarIt.Location = new System.Drawing.Point(12, 454);
            this.UnrarIt.Name = "UnrarIt";
            this.UnrarIt.Size = new System.Drawing.Size(75, 23);
            this.UnrarIt.TabIndex = 1;
            this.UnrarIt.Text = "Unrar!";
            this.UnrarIt.UseVisualStyleBackColor = true;
            this.UnrarIt.Click += new System.EventHandler(this.button1_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 480);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(693, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip";
            // 
            // Status
            // 
            this.Status.AutoSize = false;
            this.Status.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Status.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Status.Name = "Status";
            this.Status.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.Status.Size = new System.Drawing.Size(400, 17);
            this.Status.Text = "toolStripStatusLabel1";
            this.Status.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Status.Click += new System.EventHandler(this.Status_Click);
            // 
            // Progress
            // 
            this.Progress.Name = "Progress";
            this.Progress.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.Progress.Size = new System.Drawing.Size(104, 16);
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(54, 17);
            this.toolStripStatusLabel2.Spring = true;
            // 
            // StatusPasswords
            // 
            this.StatusPasswords.Name = "StatusPasswords";
            this.StatusPasswords.Size = new System.Drawing.Size(118, 17);
            this.StatusPasswords.Text = "toolStripStatusLabel3";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(693, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // OpenSettings
            // 
            this.OpenSettings.Name = "OpenSettings";
            this.OpenSettings.Size = new System.Drawing.Size(154, 22);
            this.OpenSettings.Text = "Open Settings";
            this.OpenSettings.Click += new System.EventHandler(this.openSettingsdToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(151, 6);
            // 
            // Exit
            // 
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(154, 22);
            this.Exit.Text = "Exit";
            this.Exit.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(25, 20);
            this.toolStripMenuItem1.Text = "?";
            // 
            // homepageToolStripMenuItem
            // 
            this.homepageToolStripMenuItem.Image = global::UnRarIt.Properties.Resources.homepage;
            this.homepageToolStripMenuItem.Name = "homepageToolStripMenuItem";
            this.homepageToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.homepageToolStripMenuItem.Text = "Homepage";
            this.homepageToolStripMenuItem.Click += new System.EventHandler(this.homepageToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // AddPassword
            // 
            this.AddPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AddPassword.Location = new System.Drawing.Point(93, 454);
            this.AddPassword.Name = "AddPassword";
            this.AddPassword.Size = new System.Drawing.Size(86, 23);
            this.AddPassword.TabIndex = 4;
            this.AddPassword.Text = "Add Password";
            this.AddPassword.UseVisualStyleBackColor = true;
            this.AddPassword.Click += new System.EventHandler(this.AddPassword_Click);
            // 
            // BrowseDest
            // 
            this.BrowseDest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseDest.Location = new System.Drawing.Point(657, 454);
            this.BrowseDest.Name = "BrowseDest";
            this.BrowseDest.Size = new System.Drawing.Size(24, 20);
            this.BrowseDest.TabIndex = 6;
            this.BrowseDest.Text = "...";
            this.BrowseDest.UseVisualStyleBackColor = true;
            this.BrowseDest.Click += new System.EventHandler(this.BrowseDest_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(241, 457);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Destination";
            // 
            // Dest
            // 
            this.Dest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Dest.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::UnRarIt.Properties.Settings.Default, "Dest", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.Dest.Location = new System.Drawing.Point(307, 454);
            this.Dest.Name = "Dest";
            this.Dest.ReadOnly = true;
            this.Dest.Size = new System.Drawing.Size(344, 20);
            this.Dest.TabIndex = 5;
            this.Dest.Text = global::UnRarIt.Properties.Settings.Default.Dest;
            this.Dest.TextChanged += new System.EventHandler(this.Dest_TextChanged);
            // 
            // States
            // 
            this.States.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.States.ImageSize = new System.Drawing.Size(16, 16);
            this.States.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(693, 502);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BrowseDest);
            this.Controls.Add(this.Dest);
            this.Controls.Add(this.AddPassword);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.UnrarIt);
            this.Controls.Add(this.Files);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.Text = "UnRarIt.Net";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Main_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView Files;
        private System.Windows.Forms.Button UnrarIt;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ImageList Icons;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel Status;
        private System.Windows.Forms.ToolStripProgressBar Progress;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel StatusPasswords;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem Exit;
        private System.Windows.Forms.Button AddPassword;
        private System.Windows.Forms.TextBox Dest;
        private System.Windows.Forms.Button BrowseDest;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FolderBrowserDialog BrowseDestDialog;
        private System.Windows.Forms.ToolStripMenuItem homepageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OpenSettings;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ImageList States;
    }
}

