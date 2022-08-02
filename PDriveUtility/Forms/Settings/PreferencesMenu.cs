using PDriveUtility.Services.Arma;
using PDriveUtility.Services.Local;
using PDriveUtility.Services.Settings;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDriveUtility.Forms.Settings;
public partial class PreferencesMenu : Form
{
    private readonly IArmaService _armaService;
    private readonly ILocalFileService _localFileService;
    private readonly ISettingsService _settingsService;

    public delegate void OnPreferencesSaved(PreferencesMenu sender, bool saved, bool reloadArma, bool reloadLocal);
    public event OnPreferencesSaved? PreferencesSaved;

    public PreferencesMenu(IArmaService armaService, ILocalFileService localFileService, ISettingsService settingsService)
    {
        _armaService = armaService;
        _localFileService = localFileService;
        _settingsService = settingsService;

        InitializeComponent();

        armaFolderPath.Text = _settingsService.ApplicationSettings.ArmA3Path;
        outputFolderPath.Text = _settingsService.ApplicationSettings.OutputPath;

        if (_settingsService.StartupFlags.Arma3NotFound)
            armaErrorBox.Text = "Arma 3 folder was not found during startup.";
        else
            LoadArmaMods();

        if (_settingsService.StartupFlags.OutputPathNotFound)
            outputErrorBox.Text = "Output folder was not found during startup.";

    }

    private void LoadArmaMods()
    {
        armaMods.Items.Clear();

        var workshop = Path.Join(_settingsService.ApplicationSettings.ArmA3Path, "!Workshop");
        if (Directory.Exists(workshop))
        {
            foreach (var dir in Directory.EnumerateDirectories(workshop))
            {
                var dirName = Path.GetFileName(dir);

                // Get rid of this placeholder warning.
                if (dirName == "!DO_NOT_CHANGE_FILES_IN_THESE_FOLDERS")
                    continue;

                armaMods.Items.Add(dirName, _settingsService.ApplicationSettings.Mods.Contains(dirName));
            }
        }
        else
        {
            MessageBox.Show(this, "Workshop folder not found. Please ensure a valid arma path is provided.", "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
        var arma = Path.Join(armaFolderPath.Text, "arma3.exe");
        if (!File.Exists(arma))
        {
            MessageBox.Show(this, "Arma folder not found. Please enter a valid Arma folder.", "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!Directory.Exists(outputFolderPath.Text))
        {
            MessageBox.Show(this, "Output folder not found. Please enter a valid output folder.", "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var tempA3 = _settingsService.ApplicationSettings.ArmA3Path;
        var tempOut = _settingsService.ApplicationSettings.OutputPath;
        var tempMods = _settingsService.ApplicationSettings.Mods;

        _settingsService.ApplicationSettings.ArmA3Path = armaFolderPath.Text;
        _settingsService.ApplicationSettings.OutputPath = outputFolderPath.Text;
        _settingsService.ApplicationSettings.Mods = armaMods.CheckedItems.Cast<string>().ToList();

        _ = Task.Run(async () => await _settingsService.SaveApplicationSettingsAsync());

        PreferencesSaved?.Invoke(this, true,
            (tempA3 != _settingsService.ApplicationSettings.ArmA3Path
                || tempMods != _settingsService.ApplicationSettings.Mods),
            tempOut != _settingsService.ApplicationSettings.OutputPath);

        this.Close();
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        PreferencesSaved?.Invoke(this, false, false, false);

        this.Close();
    }

    private void PickArmaFolder_Click(object sender, EventArgs e)
    {
        var res = GetFolderSelectorPath();
        armaFolderPath.Text = res;
    }

    private void PickOutputFolder_Click(object sender, EventArgs e)
    {
        var res = GetFolderSelectorPath();
        outputFolderPath.Text = res;
    }

    private void ArmaFolderPath_TextChanged(object sender, EventArgs e)
    {
        var arma = Path.Join(armaFolderPath.Text, "arma3.exe");
        if (File.Exists(arma))
        {
            armaFolderPath.ForeColor = Color.Black;
            saveButton.Enabled = true;

            if (armaFolderPath.Text != _settingsService.ApplicationSettings.ArmA3Path)
                armaErrorBox.Text = "A reload of arma files will be required.";
        }
        else
        {
            armaFolderPath.ForeColor = Color.Red;
            armaErrorBox.Text = "ArmA 3 path not found.";
            saveButton.Enabled = false;
        }
    }

    private void OutputFolderPath_TextChanged(object sender, EventArgs e)
    {
        if (Directory.Exists(outputFolderPath.Text))
        {
            outputFolderPath.ForeColor = Color.Black;
            saveButton.Enabled = true;

            if (outputFolderPath.Text != _settingsService.ApplicationSettings.OutputPath)
                outputErrorBox.Text = "A reload of output files will be required.";
        }
        else
        {
            outputFolderPath.ForeColor = Color.Red;
            outputErrorBox.Text = "Failed to find the inputed path. Please enter a valid folder path.";
            saveButton.Enabled = false;
        }
    }

    private string? GetFolderSelectorPath()
    {
        var res = folderDialouge.ShowDialog(this);
        if (res == DialogResult.OK)
        {
            return folderDialouge.SelectedPath;
        }

        return null;
    }

    private void RefreshModList_Click(object sender, EventArgs e)
    {
        LoadArmaMods();
    }
}
