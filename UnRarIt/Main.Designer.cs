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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Rar-Archives", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Zip-Archives", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.Files = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.Icons = new System.Windows.Forms.ImageList(this.components);
            this.UnrarIt = new System.Windows.Forms.Button();
            this.Statusbar = new System.Windows.Forms.StatusStrip();
            this.Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.Progress = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusPasswords = new System.Windows.Forms.ToolStripStatusLabel();
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.FileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.HelpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.About = new System.Windows.Forms.ToolStripMenuItem();
            this.AddPassword = new System.Windows.Forms.Button();
            this.BrowseDest = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.BrowseDestDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.Dest = new System.Windows.Forms.TextBox();
            this.StateIcons = new System.Windows.Forms.ImageList(this.components);
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.OpenSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.Homepage = new System.Windows.Forms.ToolStripMenuItem();
            this.Statusbar.SuspendLayout();
            this.MainMenu.SuspendLayout();
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
            listViewGroup1.Header = "Rar-Archives";
            listViewGroup1.Name = "GroupRar";
            listViewGroup2.Header = "Zip-Archives";
            listViewGroup2.Name = "GroupZip";
            this.Files.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.Files.LargeImageList = this.Icons;
            this.Files.Location = new System.Drawing.Point(12, 27);
            this.Files.Name = "Files";
            this.Files.Size = new System.Drawing.Size(669, 421);
            this.Files.SmallImageList = this.Icons;
            this.Files.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.Files.StateImageList = this.StateIcons;
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
            this.UnrarIt.Click += new System.EventHandler(this.UnRarIt_Click);
            // 
            // Statusbar
            // 
            this.Statusbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Status,
            this.Progress,
            this.toolStripStatusLabel2,
            this.StatusPasswords});
            this.Statusbar.Location = new System.Drawing.Point(0, 480);
            this.Statusbar.Name = "Statusbar";
            this.Statusbar.Size = new System.Drawing.Size(693, 22);
            this.Statusbar.TabIndex = 2;
            this.Statusbar.Text = "statusStrip";
            // 
            // Status
            // 
            this.Status.AutoSize = false;
            this.Status.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Status.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Status.Name = "Status";
            this.Status.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.Status.Size = new System.Drawing.Size(400, 17);
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
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(172, 17);
            this.toolStripStatusLabel2.Spring = true;
            // 
            // StatusPasswords
            // 
            this.StatusPasswords.Name = "StatusPasswords";
            this.StatusPasswords.Size = new System.Drawing.Size(0, 17);
            // 
            // MainMenu
            // 
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu,
            this.HelpMenu});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(693, 24);
            this.MainMenu.TabIndex = 3;
            this.MainMenu.Text = "menuStrip";
            // 
            // FileMenu
            // 
            this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenSettings,
            this.toolStripMenuItem2,
            this.Exit});
            this.FileMenu.Name = "FileMenu";
            this.FileMenu.Size = new System.Drawing.Size(37, 20);
            this.FileMenu.Text = "File";
            // 
            // Exit
            // 
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(176, 22);
            this.Exit.Text = "Exit";
            this.Exit.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // HelpMenu
            // 
            this.HelpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Homepage,
            this.About});
            this.HelpMenu.Name = "HelpMenu";
            this.HelpMenu.Size = new System.Drawing.Size(25, 20);
            this.HelpMenu.Text = "?";
            // 
            // About
            // 
            this.About.Name = "About";
            this.About.Size = new System.Drawing.Size(137, 22);
            this.About.Text = "About";
            this.About.Click += new System.EventHandler(this.About_Click);
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
            // StateIcons
            // 
            this.StateIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.StateIcons.ImageSize = new System.Drawing.Size(16, 16);
            this.StateIcons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(173, 6);
            // 
            // OpenSettings
            // 
            this.OpenSettings.Image = global::UnRarIt.Properties.Resources.preferences;
            this.OpenSettings.Name = "OpenSettings";
            this.OpenSettings.Size = new System.Drawing.Size(176, 22);
            this.OpenSettings.Text = "Open Preferences";
            this.OpenSettings.Click += new System.EventHandler(this.OpenSettings_Click);
            // 
            // Homepage
            // 
            this.Homepage.Image = global::UnRarIt.Properties.Resources.homepage;
            this.Homepage.Name = "Homepage";
            this.Homepage.Size = new System.Drawing.Size(137, 22);
            this.Homepage.Text = "Homepage";
            this.Homepage.Click += new System.EventHandler(this.Homepage_Click);
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
            this.Controls.Add(this.Statusbar);
            this.Controls.Add(this.MainMenu);
            this.Controls.Add(this.UnrarIt);
            this.Controls.Add(this.Files);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.MainMenu;
            this.Name = "Main";
            this.Text = "UnRarIt.Net";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Main_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Statusbar.ResumeLayout(false);
            this.Statusbar.PerformLayout();
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
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
        private System.Windows.Forms.StatusStrip Statusbar;
        private System.Windows.Forms.ToolStripStatusLabel Status;
        private System.Windows.Forms.ToolStripProgressBar Progress;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel StatusPasswords;
        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem FileMenu;
        private System.Windows.Forms.ToolStripMenuItem HelpMenu;
        private System.Windows.Forms.ToolStripMenuItem About;
        private System.Windows.Forms.ToolStripMenuItem Exit;
        private System.Windows.Forms.Button AddPassword;
        private System.Windows.Forms.TextBox Dest;
        private System.Windows.Forms.Button BrowseDest;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FolderBrowserDialog BrowseDestDialog;
        private System.Windows.Forms.ToolStripMenuItem Homepage;
        private System.Windows.Forms.ImageList StateIcons;
        private System.Windows.Forms.ToolStripMenuItem OpenSettings;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
    }
}

