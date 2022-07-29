using PDriveUtility.Services.Arma;
using PDriveUtility.Services.Local;
using PDriveUtility.Services.Settings;
using PDriveUtility.Structures.Init;

using Serilog.Events;

namespace PDriveUtility.Forms.Init;

partial class Startup
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    private ConsoleStartup _startup;
    private ISettingsService _settingsService;
    private IArmaService _armaService;
    private ILocalFileService _localFileService;

    public Startup(ConsoleStartup startup, ISettingsService settingsService, 
        IArmaService armaService, ILocalFileService localFileService)
    {
        _startup = startup;
        _settingsService = settingsService;
        _armaService = armaService;
        _localFileService = localFileService;

        this.Shown += Startup_Shown;

        InitializeComponent();
    }

    private void Startup_Shown(object sender, EventArgs e)
    {
        _ = Task.Run(async () => await StartupApplicationAsync());
    }   
    
    private async Task StartupApplicationAsync()
    {
        TryUpdateProgressBar(4, 1, "Loading Settings...");

        await _settingsService.InitalizeAsync();

        TryUpdateProgressBar(4, 2, "Loading ArmA 3 PBOs...");

        _armaService.Reset();

        var pboList = _armaService.GetArmAPBOs();

        if (pboList is not null)
        {
            for(int i = 0; i < pboList.Count; i++)
            {
                TryUpdateSecondaryProgressBar(pboList.Count, i, $"Reading {Path.GetFileName(pboList[i])}");
                _armaService.ReadPBO(pboList[i]);
            }

            TryUpdateSecondaryProgressBar(1, 0, "Waiting...");
        }
        else
        {
            // Set this flag for later. The main form will use it.
            _settingsService.StartupFlags.Arma3NotFound = true;
        }

        TryUpdateProgressBar(4, 3, "Reading local files...");

        // get local dirs.

        var localList = _localFileService.GetLocalDirectories();

        if (localList is not null)
        {
            for(int i = 0; i < localList.Count; i++)
            {
                TryUpdateSecondaryProgressBar(localList.Count, i, $"Processing {Path.GetFileName(localList[i])}");
                _localFileService.LoadDirectory(localList[i]);
            }

            TryUpdateSecondaryProgressBar(1, 0, "Waiting...");
        }
        else
        {
            _settingsService.StartupFlags.OutputPathNotFound = true;
        }

        // read local dirs.

        TryUpdateProgressBar(4, 4, "Finalizing...");
    }

    private void TryUpdateProgressBar(int max, int val, string text)
    {
        if (ProgressBar.InvokeRequired)
        {
            ProgressBar.Invoke(TryUpdateProgressBar, max, val, text);
            return;
        }    

        try
        {
            ProgressBar.Maximum = max;
            ProgressBar.Value = val;
            ProgressLabel.Text = text;
        }
        catch
        {
            // Do nothing.
        }
    }

    private void TryUpdateSecondaryProgressBar(int max, int val, string text)
    {
        if (SecondaryProgressBar.InvokeRequired)
        {
            SecondaryProgressBar.Invoke(TryUpdateSecondaryProgressBar, max, val, text);
            return;
        }

        try
        {
            SecondaryProgressBar.Maximum = max;
            SecondaryProgressBar.Value = val;
            SecondaryProgressLabel.Text = text;
        }
        catch
        {
            // Do nothing.
        }
    }

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

        this.Shown -= Startup_Shown;

        _startup = null;

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.TitleLabel = new System.Windows.Forms.Label();
            this.ProgressLabel = new System.Windows.Forms.Label();
            this.SecondaryProgressLabel = new System.Windows.Forms.Label();
            this.SecondaryProgressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // ProgressBar
            // 
            this.ProgressBar.Location = new System.Drawing.Point(12, 75);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(616, 23);
            this.ProgressBar.Step = 1;
            this.ProgressBar.TabIndex = 3;
            this.ProgressBar.UseWaitCursor = true;
            // 
            // TitleLabel
            // 
            this.TitleLabel.Font = new System.Drawing.Font("Cascadia Mono", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.TitleLabel.Location = new System.Drawing.Point(12, 9);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new System.Drawing.Size(616, 34);
            this.TitleLabel.TabIndex = 1;
            this.TitleLabel.Text = "P Drive Utility";
            this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.TitleLabel.UseWaitCursor = true;
            // 
            // ProgressLabel
            // 
            this.ProgressLabel.BackColor = System.Drawing.Color.Transparent;
            this.ProgressLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ProgressLabel.Location = new System.Drawing.Point(12, 49);
            this.ProgressLabel.Name = "ProgressLabel";
            this.ProgressLabel.Size = new System.Drawing.Size(616, 23);
            this.ProgressLabel.TabIndex = 4;
            this.ProgressLabel.Text = "Waiting...";
            this.ProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.ProgressLabel.UseWaitCursor = true;
            // 
            // SecondaryProgressLabel
            // 
            this.SecondaryProgressLabel.BackColor = System.Drawing.Color.Transparent;
            this.SecondaryProgressLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.SecondaryProgressLabel.Location = new System.Drawing.Point(12, 101);
            this.SecondaryProgressLabel.Name = "SecondaryProgressLabel";
            this.SecondaryProgressLabel.Size = new System.Drawing.Size(616, 23);
            this.SecondaryProgressLabel.TabIndex = 6;
            this.SecondaryProgressLabel.Text = "Waiting...";
            this.SecondaryProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.SecondaryProgressLabel.UseWaitCursor = true;
            // 
            // SecondaryProgressBar
            // 
            this.SecondaryProgressBar.Location = new System.Drawing.Point(12, 127);
            this.SecondaryProgressBar.Name = "SecondaryProgressBar";
            this.SecondaryProgressBar.Size = new System.Drawing.Size(616, 23);
            this.SecondaryProgressBar.Step = 1;
            this.SecondaryProgressBar.TabIndex = 5;
            this.SecondaryProgressBar.UseWaitCursor = true;
            // 
            // Startup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(640, 160);
            this.ControlBox = false;
            this.Controls.Add(this.SecondaryProgressLabel);
            this.Controls.Add(this.SecondaryProgressBar);
            this.Controls.Add(this.ProgressLabel);
            this.Controls.Add(this.TitleLabel);
            this.Controls.Add(this.ProgressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Startup";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Startup";
            this.TopMost = true;
            this.UseWaitCursor = true;
            this.ResumeLayout(false);

    }

    #endregion

    private ProgressBar ProgressBar;
    private Label TitleLabel;
    private Label ProgressLabel;
    private Label SecondaryProgressLabel;
    private ProgressBar SecondaryProgressBar;
}