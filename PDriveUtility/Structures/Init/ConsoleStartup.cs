using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.DependencyInjection;

using PDriveUtility.Forms.Init;
using PDriveUtility.Forms.Main;
using PDriveUtility.Forms.Settings;
using PDriveUtility.Services.Arma;
using PDriveUtility.Services.Files;
using PDriveUtility.Services.Local;
using PDriveUtility.Services.Settings;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDriveUtility.Structures.Init;

[Command(Name = "pdfs", Description = "A P Drive file system manager.")]
public class ConsoleStartup
{
    [Option("--headless", CommandOptionType.NoValue, Description = "Runs the file system in headless mode with additional parameters.")]
    public bool Headless { get; set; } = false;

    private void OnExecute()
    {
        if(Properties.Settings.Default.UpgradeNeeded)
        {
            Properties.Settings.Default.Upgrade();
            Properties.Settings.Default.UpgradeNeeded = false;
        }

        var collection = new ServiceCollection();

        Initalize(collection);

        var services = collection.BuildServiceProvider();

        var settings = services.GetRequiredService<ISettingsService>();

        // Load default logger info until we initalize the app.
        settings.ReloadLogger();

        if (Headless)
        {
            Log.Information("Starting application in headless mode.");

            // Do headless setup.

            Log.Error("This feature is not implemented yet.");
            Environment.Exit(0);
        }
        else
        {
            Log.Information("Starting application with UI.");

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            var startup = services.GetRequiredService<MainWindow>();
            
            Application.Run(startup);
        }
    }

    private void Initalize(ServiceCollection services)
    {
        // Base forms
        services.AddTransient<Startup>()
            .AddTransient<MainWindow>()
            .AddTransient<PreferencesMenu>();

        // Services
        services.AddSingleton<ISettingsService, SettingsService>()
            .AddSingleton<IArmaService, ArmaService>()
            .AddSingleton<ILocalFileService, LocalFileService>()
            .AddSingleton<IFileManagerService, FileManagerService>();

        // Utils
        services.AddSingleton(this);
    }
}
