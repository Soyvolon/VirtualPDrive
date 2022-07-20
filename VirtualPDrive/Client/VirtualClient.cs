using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VirtualMemoryProvider;

using VirtualPDrive.Core.Client;
using VirtualPDrive.PBO;

namespace VirtualPDrive.Client;
public class VirtualClient : VirtualClientBase
{
    private bool disposedValue;

    #region Events
    public override event OnStartEventHandler? OnStart;
    public override event OnShutdownEventHandler? OnShutdown;
    #endregion

    public VirtualClient(VirtualClientSettings settings)
        : base(settings)
    {
        Cancellation = new CancellationTokenSource();
    }

    public override async Task StartAsync()
    {
        var provider = Initalize();

        if (provider is null)
        {
            Log.Error("No provider was built for the virtual file system.");
            return;
        }

        await ReadArmAPBOAsync(provider);

        if (!string.IsNullOrWhiteSpace(Settings.Local))
        {
            Settings.Local = Path.Combine(Environment.CurrentDirectory, Settings.Local);
            await LoadLocalFilesAsync(provider);
        }

        Log.Information("Setup complete. Starting virtaul file server.");

        provider.StartVirtualization();

        Log.Information($"Virtual file server started at {Settings.OutputPath}");

        if (OnStart is not null)
            await OnStart.Invoke(this, new(Cancellation));

        // Keep this running until it is signaled to stop.
        try
        {
            await Task.Delay(-1, Cancellation.Token);
        }
        catch { /* wait */ }

        Log.Information("Closing file server.");

        provider.Dispose();

        if (OnShutdown is not null)
            OnShutdown?.Invoke(this);
    }

    private MemoryProvider? Initalize()
    {
        Settings.ArmAPath = Path.Combine(Environment.CurrentDirectory, Settings.ArmAPath);
        if (!Directory.Exists(Settings.ArmAPath))
        {
            Log.Error("ArmA directory not found.");
            return null;
        }

        string arma3path = Path.Combine(Settings.ArmAPath, "arma3.exe");

        if (!File.Exists(arma3path))
        {
            Log.Error("Could not find amra3.exe");
            return null;
        }

        Log.Information("Found Arma 3 - Initalizing Runner");

        Settings.OutputPath = Path.Combine(Environment.CurrentDirectory, Settings.OutputPath);

        if (!Directory.Exists(Settings.OutputPath))
        {
            Log.Information($"Creating output directory at {Settings.OutputPath}.");
            Directory.CreateDirectory(Settings.OutputPath);
        }

        var options = new MemoryProviderOptions()
        {
            VirtRoot = Settings.OutputPath,
            OutputRoot = Path.GetFileName(Settings.OutputPath)
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
        checkStack.Push(Settings.ArmAPath);

        if (!Settings.NoMods)
        {
            foreach (var dir in Directory.GetDirectories(Path.Join(Settings.ArmAPath, "!Workshop")))
            {
                var name = Path.GetFileName(dir);
                if (Settings.ModsFilter.Length > 0)
                {
                    if (Settings.ModsFilter.Any(x => x == name))
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
        Log.Information("Adding Settings.Local file system to virtual provider...");

        List<string> files = new();
        Stack<string> searchStack = new();
        searchStack.Push(Settings.Local!);

        while (searchStack.TryPop(out var search))
        {
            files.AddRange(Directory.GetFiles(search));

            foreach (var dir in Directory.GetDirectories(search))
                searchStack.Push(dir);
        }

        foreach (var file in files)
        {
            var path = Path.GetRelativePath(Settings.Local!, file);
            provider.MemorySystem.AddFile(path);
        }

        Log.Information($"...Added {files.Count} Settings.Local files.");

        return Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                Cancellation.Cancel(true);
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    public new void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
