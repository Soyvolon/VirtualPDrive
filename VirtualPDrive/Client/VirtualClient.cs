using Serilog;

using BIS.PBO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MemoryFS;
using VirtualMemoryProvider.Util;

namespace VirtualPDrive.Client;
public class VirtualClient
{
    #region Events
    public delegate Task OnInitEventHandler(object sender);
    public event OnInitEventHandler? OnInit;

    public delegate Task OnStartEventHandler(object sender);
    public event OnStartEventHandler? OnStart;

    public delegate Task OnErrorEventHandler(object sender, VirtualClientErrorEventArgs args);
    public event OnErrorEventHandler? OnError;

    public delegate Task OnShutdownEventHandler(object sender);
    public event OnShutdownEventHandler? OnShutdown;
    #endregion

    public VirtualClientSettings Settings { get; protected set; }

    private MemoryProvider? Provider { get; set; }

    private readonly CancellationTokenSource _cancellationSource;
    private readonly CancellationToken _cancellationToken;

    public VirtualClient(VirtualClientSettings settings)
    {
        Settings = settings;

        Settings.OutputPath = Path.Combine(Environment.CurrentDirectory, Settings.OutputPath);

        _cancellationSource =  new CancellationTokenSource();
        _cancellationToken = _cancellationSource.Token;
    }

    public void Start()
    {
        Initalize();

        if (Provider is null)
        {
            LogError("No provider was built for the virtual file system.");
            return;
        }

        if (OnInit is not null)
            _ = Task.Run(async () => await OnInit.Invoke(this));

        var postInit = new Task(async (x) =>
        {
            try
            {
                await PostInit();
            }
            catch (Exception ex)
            {
                Log.Error("Error occoured in Post Init: {err}", ex);
                if (OnError is not null)
                    await OnError.Invoke(this, new()
                    {
                        Exception = ex,
                        Message = ex.Message
                    });
            }
        }, null, _cancellationToken, TaskCreationOptions.LongRunning);
        postInit.Start();
    }

    private async Task PostInit()
    {
        ReadArmAPBO();

        if (!string.IsNullOrWhiteSpace(Settings.Local))
        {
            Settings.Local = Path.Combine(Environment.CurrentDirectory, Settings.Local);
            LoadLocalFiles();
        }

        Log.Information("Setup complete. Starting virtaul file server.");

        if (!Provider!.StartVirtualization())
        {
            LogError("Virtualization start failed. Another instance with the same" +
                " output route may be running.");
            return;
        }

        if (Settings.PreLoad)
        {
            Log.Information("Preloading allowed files...");

            await Provider.MemorySystem.InitalizeFileSystemAsync();

            Log.Information("File preloading complete.");
        }

        Log.Information($"Virtual file server started at {Settings.OutputPath}");

        if (OnStart is not null)
            await OnStart.Invoke(this);
    }

    private void Initalize()
    {
        Settings.ArmAPath = Path.Combine(Environment.CurrentDirectory, Settings.ArmAPath);
        if (!Directory.Exists(Settings.ArmAPath))
        {
            LogError("ArmA directory not found.");
            return;
        }

        string arma3path = Path.Combine(Settings.ArmAPath, "arma3.exe");

        if (!File.Exists(arma3path))
        {
            LogError("Could not find amra3.exe");
            return;
        }

        Log.Information("Found Arma 3 - Initalizing Runner");

        if (!Directory.Exists(Settings.OutputPath))
        {
            Log.Information($"Creating output directory at {Settings.OutputPath}.");
            Directory.CreateDirectory(Settings.OutputPath);
        }
        else if (!Settings.NoClean)
        {
            Log.Information("Cleaning output directory");
            Directory.Delete(Settings.OutputPath, true);
            Directory.CreateDirectory(Settings.OutputPath);
        }

        var options = new MemoryProviderOptions()
        {
            VirtRoot = Settings.OutputPath,
            OutputRoot = Path.GetFileName(Settings.OutputPath),
            DenyDeletes = true,
            EnableNotifications = false,

            ReadableExtensions = Settings.ReadableExtensions,
            PreloadWhitelist = Settings.PreloadWhitelist,
            InitRunners = Settings.InitRunners,
        };

        Provider = new MemoryProvider(options);

        Log.Information("Created virtual provider.");
    }

    private void ReadArmAPBO()
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

        Log.Information("Found {count} PBOs to parse.", pboPaths.Count);

        foreach (var pboPath in pboPaths)
        {
            ReadPBO(pboPath);
        }
    }

    private void ReadPBO(string pboPath)
    {
        if (Provider is null)
        {
            Log.Error("No proivder is avalible to add PBOs to.");
            return;
        }

        var pbo = new PBO(pboPath);
        if (!string.IsNullOrWhiteSpace(pbo.Prefix))
        {
            foreach (var file in pbo.FileEntries)
            {
                var root = Path.Join(pbo.Prefix, Path.GetDirectoryName(file.FileName) ?? "");

                if (root is not null)
                {
#if DEBUG
                    // Places for debugging.
                    if (root.StartsWith("ls_animation"))
                    { }
#endif
                    var rootedPath = file.FileName.CombineWithPrefix(root);

                    var res = Provider.MemorySystem.AddFile(rootedPath, file, pboPath: pbo.PBOFilePath, parentOffset: pbo.DataOffset);

                    if (res is null)
                    {
                        Log.Warning("Failed to add file: {name}", file.FileName);
                    }
                }
                else
                {
                    Log.Warning("No root was found for {name}", file.FileName);
                }
            }

            Log.Debug("Parsed {prefix}", pbo.Prefix);
        }
        else
        {
            Log.Warning("No prefix was found for {pboPath}", pboPath);
        }
    }

    private void LoadLocalFiles()
    {
        if (Provider is null)
        {
            Log.Error("No proivder is avalible to add local files to.");
            return;
        }

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
            Provider.MemorySystem.AddFile(path, null);
        }

        Log.Information("...Added {count} Settings.Local files.", files.Count);
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

    public void Dispose()
    {
        Log.Information("Closing file server.");

        _cancellationSource.Cancel();
        _cancellationSource.Dispose();

        try
        {
            Provider?.Dispose();
        }
        catch (Exception ex)
        {
            Log.Error("Failed to stop the virtual provider {err}", ex);
        }

        OnInit = null;
        OnStart = null;
        OnError = null;
        Provider = null;

        Log.Information("File server closed.");

        if (OnShutdown is not null)
            OnShutdown?.Invoke(this);

        OnShutdown = null;
    }
}