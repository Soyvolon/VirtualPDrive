using Serilog;

using BIS.PBO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MemoryFS;
using VirtualMemoryProvider.Util;
using System.Collections.Concurrent;
using System.Management;
using Microsoft.Win32;

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

    private ConcurrentDictionary<string, MemoryProvider> Providers { get; set; } = new();

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

        if (Providers is null)
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

        if (Settings.PreLoad)
        {
            Log.Information("Preloading allowed files...");

            List<Task> run = new();
            foreach(var p in Providers!.Values)
                run.Add(p.MemorySystem.InitalizeFileSystemAsync());

            await Task.WhenAll(run);

            Log.Information("File preloading complete.");
        }

        foreach (var p in Providers!.Values)
        {
            try
            {
                if (!p.StartVirtualization())
                {
                    Log.Warning("Virtualization start failed for {root}. Another instance with the same" +
                        " output route may be running.", p.Options.OutputRoot);
                    continue;
                }
            }
            catch (Exception ex)
            {
                Log.Warning("An error occoured when starting virtualization: {ex}", ex);
                continue;
            }
        }

        Log.Information($"Virtual file server started at {Settings.OutputPath}");

        if (OnStart is not null)
            await OnStart.Invoke(this);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This is windows only.")]
    private async void Initalize()
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

        Settings.OutputPath = Path.GetFullPath(Settings.OutputPath);
        if (!Directory.Exists(Settings.OutputPath))
        {
            Log.Information($"Creating output directory at {Settings.OutputPath}.");
            Directory.CreateDirectory(Settings.OutputPath);
        }

        var allDrives = DriveInfo.GetDrives();
        var drive = allDrives.FirstOrDefault(x => x.Name == Settings.OutputPath);
        if (drive is not null)
        {
            if (drive.RootDirectory.Name[..2].Equals("P:"))
            {
                // Find P Drive install location.
                var toolsPath = (string?)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Bohemia Interactive\\Arma 3 Tools", "path", "");
                if (!string.IsNullOrWhiteSpace(toolsPath))
                {
                    using var file = new StreamReader(File.OpenRead(Path.Join(toolsPath, "settings.ini")));
                    bool set = false;
                    while(!file.EndOfStream)
                    {
                        var line = file.ReadLine();
                        if (line?.StartsWith("P_DrivePath") ?? false)
                        {
                            var value = line[(line.IndexOf('"') + 1)..line.LastIndexOf('"')];

                            if (value == "/full/path/to/ArmaWork")
                            {
                                value = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Arma 3 Projects");
                            }

                            Settings.OutputPath = value;
                            set = true;
                            break;
                        }
                    }

                    if (!set)
                    {
                        var ex = new Exception("Failed to find P: drive registration in a3 tools settings.ini.");
                        LogError("Failed to find P: drive registration in a3 tools settings.ini", ex);
                        throw ex;
                    }
                }
                else
                {
                    var ex = new Exception("Failed to find A3 Tools registration.");
                    LogError("Failed to find A3 Tools registration", ex);
                    throw ex;
                }
            }
        }
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
        if (Providers is null)
        {
            Log.Error("No proivder is avalible to add PBOs to.");
            return;
        }

        var pbo = new PBO(pboPath);
        if (!string.IsNullOrWhiteSpace(pbo.Prefix))
        {
            var provider = GetOrRegisterNewProvider(pbo.Prefix);

            if (provider is not null)
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
                        var rootedPath = file.FileName.CombineWithPrefix(root, provider.Options.OutputRoot, true);

                        var res = provider.MemorySystem.AddFile(rootedPath, file, pboPath: pbo.PBOFilePath, parentOffset: pbo.DataOffset);

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
            }
            else
            {
                Log.Error("Failed to get a provider for {prefix}", pbo.Prefix);
                return;
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
        if (Providers is null)
        {
            Log.Error("No proivder is avalible to add local files to.");
            return;
        }

        Log.Information("Adding Settings.Local file system to virtual provider...");

        List<string> files = new();
        Stack<string> searchStack = new();
        searchStack.Push(Settings.Local!);
        bool notFirst = false;

        while (searchStack.TryPop(out var search))
        {
            if (notFirst)
                files.AddRange(Directory.GetFiles(search));
            else
                notFirst = true;

            foreach (var dir in Directory.GetDirectories(search))
                searchStack.Push(dir);
        }

        foreach (var file in files)
        {
            var path = Path.GetRelativePath(Settings.Local!, file);
            var provider = GetOrRegisterNewProvider(path);

            if (provider is not null)
            {
                path = string.Join(Path.DirectorySeparatorChar, path.Split(Path.DirectorySeparatorChar)[1..]);
                provider.MemorySystem.AddFile(path, null);
            }
            else
                Log.Warning("Failed to get a provider for {path}", path);
        }

        Log.Information("...Added {count} Settings.Local files.", files.Count);
    }

    private MemoryProvider? GetOrRegisterNewProvider(string path)
    {
        var rootPart = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
#if DEBUG
        if(rootPart?.StartsWith("kobra", StringComparison.OrdinalIgnoreCase) ?? false)
        { }
#endif

        if (!string.IsNullOrWhiteSpace(rootPart))
        {
            var rootKey = rootPart.ToLower();
            if (!Providers.TryGetValue(rootKey, out var provider))
            {
                var options = new MemoryProviderOptions()
                {
                    VirtRoot = Path.Join(Settings.OutputPath, rootPart),
                    OutputRoot = rootPart,
                    DenyDeletes = true,
                    EnableNotifications = false,

                    ReadableExtensions = Settings.ReadableExtensions,
                    Whitelist = Settings.Whitelist,
                    InitRunners = Settings.InitRunners,
                };

                var newProvider = new MemoryProvider(options);
                Providers[rootKey] = newProvider;
                return newProvider;
            }

            return provider;
        }
        else
        {
            Log.Error("Failed to get inital path part for {path}", path);
            return null;
        }
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

        foreach (var p in Providers.Values)
        {
            try
            {
                p.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to stop the virtual provider {name} {err}", p.Options.OutputRoot, ex);
            }
        }

        OnInit = null;
        OnStart = null;
        OnError = null;
        Providers = null;

        if (OnShutdown is not null)
            OnShutdown?.Invoke(this);

        OnShutdown = null;

        Log.Information("File server closed.");
    }
}