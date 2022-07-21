using Serilog;

using SwiftPbo;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VirtualMemoryProvider;

namespace VirtualPDrive.Client;
public class VirtualClient
{
    private bool disposedValue;

    #region Events
    public delegate Task OnStartEventHandler(object sender, VirtualClientEventArgs args);
    public event OnStartEventHandler? OnStart;

    public delegate Task OnErrorEventHandler(object sender, VirtualClientErrorEventArgs args);
    public event OnErrorEventHandler? OnError;

    public delegate Task OnShutdownEventHandler(object sender);
    public event OnShutdownEventHandler? OnShutdown;
    #endregion

    public VirtualClientSettings Settings { get; protected set; }
    public CancellationTokenSource Cancellation { get; protected set; }

    public VirtualClient(VirtualClientSettings settings)
    {
        Settings = settings;
        Cancellation = new CancellationTokenSource();
    }

    public async Task StartAsync()
    {
        var provider = Initalize();

        if (provider is null)
        {
            LogError("No provider was built for the virtual file system.");
            return;
        }

        await ReadArmAPBOAsync(provider);

        if (!string.IsNullOrWhiteSpace(Settings.Local))
        {
            Settings.Local = Path.Combine(Environment.CurrentDirectory, Settings.Local);
            await LoadLocalFilesAsync(provider);
        }

        Log.Information("Setup complete. Starting virtaul file server.");

        if(!provider.StartVirtualization())
        {
            LogError("Virtualization start failed. Another instance with the same" +
                " output route may be running.");
            return;
        }

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
            LogError("ArmA directory not found.");
            return null;
        }

        string arma3path = Path.Combine(Settings.ArmAPath, "arma3.exe");

        if (!File.Exists(arma3path))
        {
            LogError("Could not find amra3.exe");
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
        var pbo = new PboArchive(pboPath);
        if (!string.IsNullOrWhiteSpace(pbo.ProductEntry.Prefix))
        {
            foreach (var file in pbo.Files)
            {
                var root = Path.Join(pbo.ProductEntry.Prefix, Path.GetDirectoryName(file.FileName) ?? "");

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

            Log.Verbose("Parsed {prefix}", pbo.ProductEntry.Prefix);
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

    private void LogError(string msg, Exception? ex = null)
    {
        Log.Error(msg);
        _ = Task.Run(async () =>
        {
            if (OnError is not null)
            {
                await OnError.Invoke(this, new()
                {
                    Message = msg,
                    Exception = ex
                });
            }
        });
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

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}