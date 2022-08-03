
using PDriveUtility.Services.Arma;
using PDriveUtility.Services.Files;
using PDriveUtility.Services.Local;
using PDriveUtility.Services.Settings;
using PDriveUtility.Structures.Init;

namespace PDriveUtility.Forms.Init;
public partial class Startup : Form
{

    private readonly ConsoleStartup _startup;
    private readonly ISettingsService _settingsService;
    private readonly IArmaService _armaService;
    private readonly ILocalFileService _localFileService;
    private readonly IServiceProvider _services;
    private readonly IFileManagerService _fileManagerService;

    public delegate void OnStartupCompleted(object sender);
    public event OnStartupCompleted? StartupCompleted;

    public Startup(ConsoleStartup startup, ISettingsService settingsService,
        IArmaService armaService, ILocalFileService localFileService, IServiceProvider services,
        IFileManagerService fileManagerService)
    {
        _startup = startup;
        _settingsService = settingsService;
        _armaService = armaService;
        _localFileService = localFileService;
        _services = services;
        _fileManagerService = fileManagerService;

        this.Shown += Startup_Shown;

        InitializeComponent();
    }

    private void Startup_Shown(object? sender, EventArgs e)
    {
        _ = Task.Run(async () => await StartupApplicationAsync());
    }

    private async Task StartupApplicationAsync()
    {
        TryUpdateProgressBar(4, 1, "Loading Settings...");

        await _settingsService.InitalizeAsync();

        TryUpdateProgressBar(4, 2, "Loading ArmA 3 PBOs...");

        if (!_settingsService.StartupFlags.SkipArma3)
        {
            _armaService.Reset();

            var pboList = _armaService.GetArmAPBOs();

            if (pboList is not null)
            {
                for (int i = 0; i < pboList.Count; i++)
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
        }

        TryUpdateProgressBar(4, 3, "Reading local files...");

        if (!_settingsService.StartupFlags.SkipOutput)
        {
            // get local dirs.
            var localList = _localFileService.GetLocalDirectories();

            // Read local dirs.
            if (localList is not null)
            {
                for (int i = 0; i < localList.Count; i++)
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
        }

        TryUpdateProgressBar(4, 4, "Finalizing...");

        _fileManagerService.Initalize();

        StartupCompleted?.Invoke(this);

        this.Invoke(this.Close);
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
}
