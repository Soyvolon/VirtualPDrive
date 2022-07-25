// See https://aka.ms/new-console-template for more information

using McMaster.Extensions.CommandLineUtils;

using Serilog;

using MemoryFS;

using VirtualPDrive.Client;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;
using System.Security.Principal;
using System.Reflection;

namespace VirtualPDrive.Runner;

[Command(Name = "vpdrive", Description = "A virtual ArmA 3 P drive builder.")]
public class Program
{
    public static async Task Main(string[] args)
    {
        await CommandLineApplication.ExecuteAsync<Program>(args);
    }

    [Argument(0, Description = "ArmA 3 Path")]
    private string ArmAPath { get; set; } = "";

    [Option("-m|--mod", CommandOptionType.MultipleValue, Description = "Filters extracted mods to those passed" +
        " as a parameter.")]
    private string[] ModsFilter { get; set; } = Array.Empty<string>();

    [Option("--no-mods", CommandOptionType.NoValue, Description = "Tells the app not to load any mods.")]
    private bool NoMods { get; set; } = false;

    [Option("-o|--output", CommandOptionType.SingleValue, Description = "Output path for this command.")]
    private string OutputPath { get; set; } = "output";

    [Option("-l|--local", CommandOptionType.SingleValue, Description = "Local file tree to insert into the output.")]
    private string? Local { get; set; } = null;

    [Option("-e|--extension", CommandOptionType.MultipleValue, Description = "Allowed file extenions for file loading.")]
    private string[] Extensions { get; set; } = Array.Empty<string>();

    [Option("--load-whitelist", CommandOptionType.MultipleValue, Description = "Allowed file names for file laoding.")]
    private string[] Whitelist { get; set; } = Array.Empty<string>();

    [Option("-p|--preload", CommandOptionType.NoValue, Description = "Set to initalize all allowed file extensions on load.")]
    private bool PreLoad { get; set; } = false;
    [Option("--init-runners", CommandOptionType.SingleValue, Description = "Set the ammount of initalize file instances that can run at one time.")]
    public int InitRunners { get; set; } = 2;

    private VirtualClient? Client { get; set; }

    private async Task OnExecute()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .CreateLogger();

        Client = new VirtualClient(new()
        {
            ArmAPath = ArmAPath,
            ModsFilter = ModsFilter,
            NoMods = NoMods,
            OutputPath = OutputPath,
            Local = Local,
            ReadableExtensions = Extensions,
            Whitelist = Whitelist,
            PreLoad = PreLoad,
            InitRunners = InitRunners
        });

        Client.OnStart += Client_OnStart;
        Client.OnShutdown += Client_OnShutdown;
        Client.OnError += Client_OnError;
        try
        {
            Client.Start();
        }
        catch (Exception ex)
        {
            Log.Error("Client encountered an error: {ex}", ex);
        }

        var cancel = new CancellationTokenSource();

        Console.CancelKeyPress += (x, y) =>
        {
            Client.Dispose();
            cancel.Cancel();
        };

        await Task.Delay(-1, cancel.Token);
    }

    private Task Client_OnError(object sender, VirtualClientErrorEventArgs args)
    {
        Log.Error("Something went wrong: {args}", args);
        Environment.Exit(-1);

        return Task.CompletedTask;
    }

    private Task Client_OnShutdown(object sender)
    {
        Log.Information("Virtual client stopped. Exiting...");
        Environment.Exit(0);

        return Task.CompletedTask;
    }

    private Task Client_OnStart(object sender)
    {
        Log.Information("Virtual client started.");

        return Task.CompletedTask;
    }
}