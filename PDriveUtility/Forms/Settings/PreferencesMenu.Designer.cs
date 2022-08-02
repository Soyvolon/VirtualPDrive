namespace PDriveUtility.Forms.Settings;

partial class PreferencesMenu
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
            this.folderDialouge = new System.Windows.Forms.FolderBrowserDialog();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.toolTipManager = new System.Windows.Forms.ToolTip(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.armaFolderPathLabel = new System.Windows.Forms.Label();
            this.armaMods = new System.Windows.Forms.CheckedListBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.armaSettings = new System.Windows.Forms.TabControl();
            this.armaPathTab = new System.Windows.Forms.TabPage();
            this.armaErrorBox = new System.Windows.Forms.Label();
            this.pickArmaFolder = new System.Windows.Forms.Button();
            this.armaFolderPath = new System.Windows.Forms.TextBox();
            this.armaModsTab = new System.Windows.Forms.TabPage();
            this.refreshModList = new System.Windows.Forms.Button();
            this.modsLabel = new System.Windows.Forms.Label();
            this.outputErrorBox = new System.Windows.Forms.Label();
            this.pickOutputFolder = new System.Windows.Forms.Button();
            this.outputFolderPath = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.armaSettings.SuspendLayout();
            this.armaPathTab.SuspendLayout();
            this.armaModsTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // folderDialouge
            // 
            this.folderDialouge.RootFolder = System.Environment.SpecialFolder.MyDocuments;
            // 
            // saveButton
            // 
            this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveButton.Image = global::PDriveUtility.Properties.Resources.GF_Save;
            this.saveButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.saveButton.Location = new System.Drawing.Point(552, 388);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(60, 25);
            this.saveButton.TabIndex = 2;
            this.saveButton.Text = "Save";
            this.saveButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Image = global::PDriveUtility.Properties.Resources.GF_Cancel;
            this.cancelButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cancelButton.Location = new System.Drawing.Point(470, 388);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(76, 25);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 24);
            this.label2.TabIndex = 10;
            this.label2.Text = "Output Folder";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.toolTipManager.SetToolTip(this.label2, "Path to the file output. Should be the P:\\ drive, but can be another folder.");
            // 
            // armaFolderPathLabel
            // 
            this.armaFolderPathLabel.Location = new System.Drawing.Point(6, 3);
            this.armaFolderPathLabel.Name = "armaFolderPathLabel";
            this.armaFolderPathLabel.Size = new System.Drawing.Size(91, 24);
            this.armaFolderPathLabel.TabIndex = 10;
            this.armaFolderPathLabel.Text = "ArmA 3 Folder";
            this.armaFolderPathLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.toolTipManager.SetToolTip(this.armaFolderPathLabel, "The path to the folder that contains Arma3.exe.\r\nCommonly found in your steamapps" +
        "\\common folder.");
            // 
            // armaMods
            // 
            this.armaMods.AllowDrop = true;
            this.armaMods.FormattingEnabled = true;
            this.armaMods.Location = new System.Drawing.Point(6, 44);
            this.armaMods.Name = "armaMods";
            this.armaMods.Size = new System.Drawing.Size(580, 184);
            this.armaMods.TabIndex = 0;
            this.toolTipManager.SetToolTip(this.armaMods, "Select the mods to load when loading files.");
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.armaSettings);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.outputErrorBox);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.pickOutputFolder);
            this.splitContainer1.Panel2.Controls.Add(this.outputFolderPath);
            this.splitContainer1.Size = new System.Drawing.Size(600, 370);
            this.splitContainer1.SplitterDistance = 262;
            this.splitContainer1.TabIndex = 4;
            // 
            // armaSettings
            // 
            this.armaSettings.Controls.Add(this.armaPathTab);
            this.armaSettings.Controls.Add(this.armaModsTab);
            this.armaSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.armaSettings.Location = new System.Drawing.Point(0, 0);
            this.armaSettings.Name = "armaSettings";
            this.armaSettings.SelectedIndex = 0;
            this.armaSettings.Size = new System.Drawing.Size(600, 262);
            this.armaSettings.TabIndex = 0;
            // 
            // armaPathTab
            // 
            this.armaPathTab.Controls.Add(this.armaErrorBox);
            this.armaPathTab.Controls.Add(this.armaFolderPathLabel);
            this.armaPathTab.Controls.Add(this.pickArmaFolder);
            this.armaPathTab.Controls.Add(this.armaFolderPath);
            this.armaPathTab.Location = new System.Drawing.Point(4, 24);
            this.armaPathTab.Name = "armaPathTab";
            this.armaPathTab.Padding = new System.Windows.Forms.Padding(3);
            this.armaPathTab.Size = new System.Drawing.Size(592, 234);
            this.armaPathTab.TabIndex = 0;
            this.armaPathTab.Text = "ArmA Path";
            this.armaPathTab.UseVisualStyleBackColor = true;
            // 
            // armaErrorBox
            // 
            this.armaErrorBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.armaErrorBox.ForeColor = System.Drawing.Color.Red;
            this.armaErrorBox.Location = new System.Drawing.Point(6, 188);
            this.armaErrorBox.Name = "armaErrorBox";
            this.armaErrorBox.Size = new System.Drawing.Size(576, 43);
            this.armaErrorBox.TabIndex = 11;
            // 
            // pickArmaFolder
            // 
            this.pickArmaFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pickArmaFolder.Image = global::PDriveUtility.Properties.Resources.GF_FolderOpen;
            this.pickArmaFolder.Location = new System.Drawing.Point(549, 30);
            this.pickArmaFolder.Name = "pickArmaFolder";
            this.pickArmaFolder.Size = new System.Drawing.Size(33, 23);
            this.pickArmaFolder.TabIndex = 9;
            this.pickArmaFolder.UseVisualStyleBackColor = true;
            // 
            // armaFolderPath
            // 
            this.armaFolderPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.armaFolderPath.Location = new System.Drawing.Point(6, 30);
            this.armaFolderPath.Name = "armaFolderPath";
            this.armaFolderPath.Size = new System.Drawing.Size(537, 23);
            this.armaFolderPath.TabIndex = 8;
            // 
            // armaModsTab
            // 
            this.armaModsTab.Controls.Add(this.refreshModList);
            this.armaModsTab.Controls.Add(this.modsLabel);
            this.armaModsTab.Controls.Add(this.armaMods);
            this.armaModsTab.Location = new System.Drawing.Point(4, 24);
            this.armaModsTab.Name = "armaModsTab";
            this.armaModsTab.Padding = new System.Windows.Forms.Padding(3);
            this.armaModsTab.Size = new System.Drawing.Size(592, 234);
            this.armaModsTab.TabIndex = 1;
            this.armaModsTab.Text = "ArmA Mods";
            this.armaModsTab.UseVisualStyleBackColor = true;
            // 
            // refreshModList
            // 
            this.refreshModList.Image = global::PDriveUtility.Properties.Resources.GF_Refresh;
            this.refreshModList.Location = new System.Drawing.Point(551, 6);
            this.refreshModList.Name = "refreshModList";
            this.refreshModList.Size = new System.Drawing.Size(33, 23);
            this.refreshModList.TabIndex = 3;
            this.refreshModList.UseVisualStyleBackColor = true;
            this.refreshModList.Click += new System.EventHandler(this.RefreshModList_Click);
            // 
            // modsLabel
            // 
            this.modsLabel.AutoSize = true;
            this.modsLabel.Location = new System.Drawing.Point(6, 6);
            this.modsLabel.Name = "modsLabel";
            this.modsLabel.Size = new System.Drawing.Size(114, 15);
            this.modsLabel.TabIndex = 2;
            this.modsLabel.Text = "Select Mods to Load";
            // 
            // outputErrorBox
            // 
            this.outputErrorBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputErrorBox.ForeColor = System.Drawing.Color.Red;
            this.outputErrorBox.Location = new System.Drawing.Point(12, 53);
            this.outputErrorBox.Name = "outputErrorBox";
            this.outputErrorBox.Size = new System.Drawing.Size(576, 29);
            this.outputErrorBox.TabIndex = 11;
            // 
            // pickOutputFolder
            // 
            this.pickOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pickOutputFolder.Image = global::PDriveUtility.Properties.Resources.GF_FolderOpen;
            this.pickOutputFolder.Location = new System.Drawing.Point(555, 26);
            this.pickOutputFolder.Name = "pickOutputFolder";
            this.pickOutputFolder.Size = new System.Drawing.Size(33, 23);
            this.pickOutputFolder.TabIndex = 9;
            this.pickOutputFolder.UseVisualStyleBackColor = true;
            this.pickOutputFolder.Click += new System.EventHandler(this.PickOutputFolder_Click);
            // 
            // outputFolderPath
            // 
            this.outputFolderPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputFolderPath.Location = new System.Drawing.Point(12, 27);
            this.outputFolderPath.Name = "outputFolderPath";
            this.outputFolderPath.Size = new System.Drawing.Size(537, 23);
            this.outputFolderPath.TabIndex = 8;
            this.outputFolderPath.TextChanged += new System.EventHandler(this.OutputFolderPath_TextChanged);
            // 
            // PreferencesMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 425);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "PreferencesMenu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Preferences";
            this.TopMost = true;
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.armaSettings.ResumeLayout(false);
            this.armaPathTab.ResumeLayout(false);
            this.armaPathTab.PerformLayout();
            this.armaModsTab.ResumeLayout(false);
            this.armaModsTab.PerformLayout();
            this.ResumeLayout(false);

    }

    #endregion
    private FolderBrowserDialog folderDialouge;
    private Button saveButton;
    private Button cancelButton;
    private ToolTip toolTipManager;
    private SplitContainer splitContainer1;
    private Label outputErrorBox;
    private Label label2;
    private Button pickOutputFolder;
    private TextBox outputFolderPath;
    private TabControl armaSettings;
    private TabPage armaPathTab;
    private Label armaErrorBox;
    private Label armaFolderPathLabel;
    private Button pickArmaFolder;
    private TextBox armaFolderPath;
    private TabPage armaModsTab;
    private CheckedListBox armaMods;
    private Label modsLabel;
    private Button refreshModList;
}