// See https://aka.ms/new-console-template for more information

using McMaster.Extensions.CommandLineUtils;

using Serilog;

using VirtualMemoryProvider;

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

        var provider = Initalize();

        if (provider is null)
        {
            Log.Error("No provider was built for the virtual file system.");
            return;
        }

        await ReadArmAPBOAsync(provider);

        if (!string.IsNullOrWhiteSpace(Local))
        {
            Local = Path.Combine(Environment.CurrentDirectory, Local);
            await LoadLocalFilesAsync(provider);
        }

        Log.Information("Setup complete. Starting virtaul file server.");

        provider.StartVirtualization();

        Log.Information($"Virtual file server started at {OutputPath}");

        // Keep this running.
        await Task.Delay(-1);
    }

    private MemoryProvider? Initalize()
    {
        ArmAPath = Path.Combine(Environment.CurrentDirectory, ArmAPath);
        if (!Directory.Exists(ArmAPath))
        {
            Log.Error("ArmA directory not found.");
            return null;
        }

        string arma3path = Path.Combine(ArmAPath, "arma3.exe");

        if (!File.Exists(arma3path))
        {
            Log.Error("Could not find amra3.exe");
            return null;
        }

        Log.Information("Found Arma 3 - Initalizing Runner");

        OutputPath = Path.Combine(Environment.CurrentDirectory, OutputPath);

        if (!Directory.Exists(OutputPath))
        {
            Log.Information($"Creating output directory at {OutputPath}.");
            Directory.CreateDirectory(OutputPath);
        }

        var options = new MemoryProviderOptions()
        {
            VirtRoot = OutputPath,
            OutputRoot = Path.GetFileName(OutputPath)
        };

        var provider = new MemoryProvider(options);

        Log.Information("Starting virtual provider.");

        return provider;
    }

    private async Task ReadArmAPBOAsync(MemoryProvider provider)
    {
        Log.Information("Starting ArmA 3 PBO read...");

        List<string> pboFolders = new();
        Stack<string> checkStack = new();
        checkStack.Push(ArmAPath);

        if (!NoMods)
        {
            foreach (var dir in Directory.GetDirectories(Path.Join(ArmAPath, "!Workshop")))
            {
                var name = Path.GetFileName(dir);
                if (ModsFilter.Length > 0)
                {
                    if (ModsFilter.Any(x => x == name))
                        checkStack.Push(dir);
                }
                else
                {
                    checkStack.Push(dir);
                }
            }
        }

        while (checkStack.TryPop(out var dirRoot))
        {
            foreach (var dir in Directory.GetDirectories(dirRoot))
            {
                var name = Path.GetFileName(dir);
                if (name.Equals("!Workshop", StringComparison.OrdinalIgnoreCase)) continue;

                if (name.Equals("addons", StringComparison.OrdinalIgnoreCase))
                    pboFolders.Add(dir);
                else
                    checkStack.Push(dir);
            }
        }

        List<string> pboPaths = new();
        foreach (var dir in pboFolders)
        {
            var files = Directory.GetFiles(dir, "*.pbo");
            pboPaths.AddRange(files);
        }

        Log.Information($"Found {pboPaths.Count} PBOs to parse.");

        foreach (var pboPath in pboPaths)
        {
            await ReadPBO(pboPath, provider);
        }
    }

    private async Task ReadPBO(string pboPath, MemoryProvider provider)
    {
        var pbo = await PBOClient.LoadPBOAsync(pboPath);
        if (pbo.Headers.TryGetValue("prefix", out var prefix))
        {
            foreach (var file in pbo.Files)
        {
            
                var root = Path.Join(prefix, Path.GetDirectoryName(file.RelativePath) ?? "");

                if (root is not null)
                {
                    var rootedPath = Path.Join(root, file.FileName);

                    var res = provider.MemorySystem.AddFile(rootedPath);

                    if (res is null)
                    {
                        Log.Warning($"Failed to add file: {file.FileName}");
                    }
                }
                else
                {
                    Log.Warning($"No root was found for {file.FileName}");
                }
            }

            Log.Information($"Parsed {prefix}");
        }
        else
        {
            Log.Warning($"No prefix was found for {pboPath}");
        }
    }

    private Task LoadLocalFilesAsync(MemoryProvider provider)
    {
        Log.Information("Adding local file system to virtual provider...");

        List<string> files = new();
        Stack<string> searchStack = new();
        searchStack.Push(Local!);

        while (searchStack.TryPop(out var search))
        {
            files.AddRange(Directory.GetFiles(search));

            foreach (var dir in Directory.GetDirectories(search))
                searchStack.Push(dir);
        }

        foreach(var file in files)
        {
            var path = Path.GetRelativePath(Local!, file);
            provider.MemorySystem.AddFile(path);
        }

        Log.Information($"...Added {files.Count} local files.");

        return Task.CompletedTask;
    }
}
