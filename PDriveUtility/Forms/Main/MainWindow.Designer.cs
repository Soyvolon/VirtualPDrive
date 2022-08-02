using Microsoft.Extensions.DependencyInjection;

using PDriveUtility.Forms.Init;
using PDriveUtility.Forms.Settings;

namespace PDriveUtility.Forms.Main;

partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.activeDirWatcher = new System.IO.FileSystemWatcher();
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainSpilt = new System.Windows.Forms.SplitContainer();
            this.fileActionSplit = new System.Windows.Forms.SplitContainer();
            this.fileTree = new System.Windows.Forms.TreeView();
            this.fileStatusImageList = new System.Windows.Forms.ImageList(this.components);
            this.logOutput = new System.Windows.Forms.ListBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.primaryActionBar = new System.Windows.Forms.ToolStripProgressBar();
            this.fileIconImageList = new System.Windows.Forms.ImageList(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.activeDirWatcher)).BeginInit();
            this.mainMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSpilt)).BeginInit();
            this.mainSpilt.Panel1.SuspendLayout();
            this.mainSpilt.Panel2.SuspendLayout();
            this.mainSpilt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileActionSplit)).BeginInit();
            this.fileActionSplit.Panel1.SuspendLayout();
            this.fileActionSplit.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // activeDirWatcher
            // 
            this.activeDirWatcher.EnableRaisingEvents = true;
            this.activeDirWatcher.SynchronizingObject = this;
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.preferencesToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Size = new System.Drawing.Size(838, 24);
            this.mainMenuStrip.TabIndex = 0;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // preferencesToolStripMenuItem
            // 
            this.preferencesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileConfigurationToolStripMenuItem});
            this.preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
            this.preferencesToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.preferencesToolStripMenuItem.Text = "Preferences";
            // 
            // fileConfigurationToolStripMenuItem
            // 
            this.fileConfigurationToolStripMenuItem.Name = "fileConfigurationToolStripMenuItem";
            this.fileConfigurationToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.fileConfigurationToolStripMenuItem.Text = "File Configuration";
            this.fileConfigurationToolStripMenuItem.Click += new System.EventHandler(this.FileConfigurationToolStripMenuItem_Click);
            // 
            // mainSpilt
            // 
            this.mainSpilt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mainSpilt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSpilt.Location = new System.Drawing.Point(0, 24);
            this.mainSpilt.Name = "mainSpilt";
            this.mainSpilt.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // mainSpilt.Panel1
            // 
            this.mainSpilt.Panel1.Controls.Add(this.fileActionSplit);
            // 
            // mainSpilt.Panel2
            // 
            this.mainSpilt.Panel2.Controls.Add(this.logOutput);
            this.mainSpilt.Panel2.Controls.Add(this.statusStrip);
            this.mainSpilt.Size = new System.Drawing.Size(838, 417);
            this.mainSpilt.SplitterDistance = 279;
            this.mainSpilt.TabIndex = 1;
            // 
            // fileActionSplit
            // 
            this.fileActionSplit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.fileActionSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileActionSplit.Location = new System.Drawing.Point(0, 0);
            this.fileActionSplit.Name = "fileActionSplit";
            // 
            // fileActionSplit.Panel1
            // 
            this.fileActionSplit.Panel1.Controls.Add(this.fileTree);
            this.fileActionSplit.Size = new System.Drawing.Size(838, 279);
            this.fileActionSplit.SplitterDistance = 500;
            this.fileActionSplit.TabIndex = 0;
            // 
            // fileTree
            // 
            this.fileTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileTree.FullRowSelect = true;
            this.fileTree.ImageIndex = 0;
            this.fileTree.ImageList = this.fileIconImageList;
            this.fileTree.Location = new System.Drawing.Point(0, 0);
            this.fileTree.Name = "fileTree";
            this.fileTree.SelectedImageIndex = 0;
            this.fileTree.Size = new System.Drawing.Size(498, 277);
            this.fileTree.StateImageList = this.fileStatusImageList;
            this.fileTree.TabIndex = 0;
            // 
            // fileStatusImageList
            // 
            this.fileStatusImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.fileStatusImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("fileStatusImageList.ImageStream")));
            this.fileStatusImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.fileStatusImageList.Images.SetKeyName(0, "circle_FILL0_wght400_GRAD0_opsz20.png");
            this.fileStatusImageList.Images.SetKeyName(1, "change_circle_FILL0_wght400_GRAD0_opsz20.png");
            this.fileStatusImageList.Images.SetKeyName(2, "task_alt_FILL0_wght400_GRAD0_opsz20.png");
            this.fileStatusImageList.Images.SetKeyName(3, "downloading_FILL0_wght400_GRAD0_opsz20.png");
            this.fileStatusImageList.Images.SetKeyName(4, "error_FILL0_wght400_GRAD0_opsz20.png");
            // 
            // logOutput
            // 
            this.logOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logOutput.FormattingEnabled = true;
            this.logOutput.ItemHeight = 15;
            this.logOutput.Location = new System.Drawing.Point(0, 0);
            this.logOutput.Name = "logOutput";
            this.logOutput.Size = new System.Drawing.Size(836, 110);
            this.logOutput.TabIndex = 1;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.primaryActionBar});
            this.statusStrip.Location = new System.Drawing.Point(0, 110);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(836, 22);
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // primaryActionBar
            // 
            this.primaryActionBar.Name = "primaryActionBar";
            this.primaryActionBar.Size = new System.Drawing.Size(100, 16);
            // 
            // fileIconImageList
            // 
            this.fileIconImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.fileIconImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("fileIconImageList.ImageStream")));
            this.fileIconImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.fileIconImageList.Images.SetKeyName(0, "folder_FILL0_wght400_GRAD0_opsz20.png");
            this.fileIconImageList.Images.SetKeyName(1, "settings_FILL0_wght400_GRAD0_opsz20.png");
            this.fileIconImageList.Images.SetKeyName(2, "feed_FILL0_wght400_GRAD0_opsz20.png");
            this.fileIconImageList.Images.SetKeyName(3, "image_FILL0_wght400_GRAD0_opsz20.png");
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(838, 441);
            this.Controls.Add(this.mainSpilt);
            this.Controls.Add(this.mainMenuStrip);
            this.Enabled = false;
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "P Drive Utility";
            ((System.ComponentModel.ISupportInitialize)(this.activeDirWatcher)).EndInit();
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.mainSpilt.Panel1.ResumeLayout(false);
            this.mainSpilt.Panel2.ResumeLayout(false);
            this.mainSpilt.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSpilt)).EndInit();
            this.mainSpilt.ResumeLayout(false);
            this.fileActionSplit.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fileActionSplit)).EndInit();
            this.fileActionSplit.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private FileSystemWatcher activeDirWatcher;
    private MenuStrip mainMenuStrip;
    private ToolStripMenuItem preferencesToolStripMenuItem;
    private ToolStripMenuItem fileConfigurationToolStripMenuItem;
    private SplitContainer mainSpilt;
    private SplitContainer fileActionSplit;
    private StatusStrip statusStrip;
    private ToolStripProgressBar primaryActionBar;
    private ListBox logOutput;
    private TreeView fileTree;
    private ImageList fileStatusImageList;
    private ImageList fileIconImageList;
}