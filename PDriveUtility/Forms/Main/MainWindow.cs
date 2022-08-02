using Microsoft.Extensions.DependencyInjection;

using PDriveUtility.Forms.Init;
using PDriveUtility.Forms.Settings;
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

namespace PDriveUtility.Forms.Main;
public partial class MainWindow : Form
{
    private readonly IServiceProvider _services;
    private readonly ISettingsService _settingsService;
    private readonly IArmaService _armaService;
    private readonly ILocalFileService _localFileService;

    public MainWindow(IServiceProvider services, ISettingsService settingsService,
        IArmaService armaService, ILocalFileService localFileService)
    {
        _services = services;
        _settingsService = settingsService;
        _armaService = armaService;
        _localFileService = localFileService;

        this.Shown += Main_Shown;

        InitializeComponent();
    }

    private void LoadMainWindow()
    {

    }

    private void Main_Shown(object? sender, EventArgs e)
    {
        var startup = _services.GetRequiredService<Startup>();
        startup.StartupCompleted += Startup_StartupCompleted;
        startup.Show(this);
    }

    private void Startup_StartupCompleted(object sender)
    {
        this.Invoke(() => this.Enabled = true);

        (sender as Startup)!.StartupCompleted -= Startup_StartupCompleted;

        if (_settingsService.StartupFlags.Arma3NotFound
            || _settingsService.StartupFlags.OutputPathNotFound)
        {
            this.Invoke(FileConfigurationToolStripMenuItem_Click, this, new EventArgs());
        }
        else
        {
            _ = Task.Run(() => this.Invoke(LoadMainWindow));
        }
    }

    private void Pref_PreferencesSaved(PreferencesMenu sender, bool saved, bool reloadArma, bool reloadLocal)
    {
        sender.PreferencesSaved -= Pref_PreferencesSaved;

        if (saved && (reloadArma || reloadLocal))
        {
            _settingsService.StartupFlags.SkipArma3 = !reloadArma;
            _settingsService.StartupFlags.SkipOutput = !reloadLocal;

            this.Invoke(Main_Shown, this, new EventArgs());
        }
    }

    private void FileConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var pref = _services.GetRequiredService<PreferencesMenu>();
        pref.PreferencesSaved += Pref_PreferencesSaved;
        pref.Show(this);
    }
}
