using Microsoft.Win32;

using PDriveUtility.Structures.Settings;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PDriveUtility.Services.Settings;
public class SettingsService : ISettingsService
{
    public LoggingSettings LoggingSettings { get; set; } = new();
    public ApplicationSettings ApplicationSettings { get; set; } = new();
    public StartupFlags StartupFlags { get; set; } = new();

    public SettingsService()
    {

    }

    public async Task InitalizeAsync()
    {
        var settingsPath = Properties.Settings.Default.SettingsPath;

        Directory.CreateDirectory(settingsPath);

        var appSettingsPath = Path.Join(settingsPath, "settings.json");
        ApplicationSettings? newAppSettings = null;
        if (File.Exists(appSettingsPath))
        {
            // Read settings.
            await using FileStream fs = new(appSettingsPath, FileMode.Open);
            newAppSettings = await JsonSerializer.DeserializeAsync<ApplicationSettings>(fs);
        }
        
        if (newAppSettings is null)
        {
            // Try and get the Arma path.
            var steamPath = (string?)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam", "SteamPath", "");
            var armaPath = Path.Join(steamPath, @"steamapps\common\Arma 3\arma3.exe");
            if (File.Exists(armaPath))
            {
                ApplicationSettings.ArmA3Path = Path.GetDirectoryName(armaPath) ?? "";
            }

            // Initalize settings.
            await using FileStream fs = new(appSettingsPath, FileMode.Create);
            await JsonSerializer.SerializeAsync(fs, ApplicationSettings);
        }
        else
        {
            ApplicationSettings = newAppSettings;
        }

        var logSettingsPath = Path.Join(settingsPath, "settings.loging.json");
        LoggingSettings? newLogSettings = null;
        if (File.Exists(logSettingsPath))
        {
            // Read settings.
            await using FileStream fs = new(logSettingsPath, FileMode.Open);
            newLogSettings = await JsonSerializer.DeserializeAsync<LoggingSettings>(fs);
        }

        if (newLogSettings is null)
        {
            // Initalize settings.
            await using FileStream fs = new(logSettingsPath, FileMode.Create);
            await JsonSerializer.SerializeAsync(fs, LoggingSettings);
        }
        else
        {
            LoggingSettings = newLogSettings;
        }

        ReloadLogger();
    }

    public void ReloadLogger()
    {
        var cfg = new LoggerConfiguration()
            .MinimumLevel.Verbose();

        if (LoggingSettings.WriteToConsole)
        {
            cfg.WriteTo.Console(LoggingSettings.ConsoleLevel);
        }

        if (LoggingSettings.WriteToFile)
        {
            cfg.WriteTo.Async((x) =>
            {
                x.File(Path.Join(LoggingSettings.LogFolderPath, "p-drive-util.log"),
                    rollOnFileSizeLimit: true,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: LoggingSettings.RollingFiles,
                    restrictedToMinimumLevel: LoggingSettings.FileLevel);
            });
        }

        Log.Logger = cfg.CreateLogger();
    }
}
