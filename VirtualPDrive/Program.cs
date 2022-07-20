// See https://aka.ms/new-console-template for more information

using McMaster.Extensions.CommandLineUtils;

using Serilog;

using VirtualMemoryProvider;

using VirtualPDrive.Client;
using VirtualPDrive.PBO;

namespace VirtualPDrive;

[Command(Name = "vpdrive", Description = "A virtual ArmA 3 P drive builder.")]
public class Program
{
    public static async Task Main(string[] args)
            => await CommandLineApplication.ExecuteAsync<Program>(args);

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

    private CancellationTokenSource? CancellationToken { get; set; }

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

        var client = new VirtualClient(new()
        {
            ArmAPath = ArmAPath,
            ModsFilter = ModsFilter,
            NoMods = NoMods,
            OutputPath = OutputPath,
            Local = Local
        });

        Console.CancelKeyPress += Console_CancelKeyPress;
        client.OnStart += Client_OnStart;
        client.OnShutdown += Client_OnShutdown;

        await client.StartAsync();

        await Task.Delay(-1);
    }

    private Task Client_OnShutdown(object sender)
    {
        Log.Information("Virtual client stopped. Exiting...");
        Environment.Exit(0);

        return Task.CompletedTask;
    }

    private Task Client_OnStart(object sender, VirtualClientEventArgs args)
    {
        Log.Information("Virtual client started.");
        CancellationToken = args.ClientCancellationToken;

        return Task.CompletedTask;
    }

    private void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        if (CancellationToken is not null)
            CancellationToken.Cancel(true);
    }
}