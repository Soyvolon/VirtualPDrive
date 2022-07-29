using BIS.PBO;

using PDriveFileSystem.FileSystem;
using PDriveFileSystem.Util;

using PDriveUtility.Services.Settings;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDriveUtility.Services.Arma;
public class ArmaService : IArmaService
{
    private readonly ISettingsService _settingsService;
    public MemoryFileSystem FileSystem { get; set; }

    public ArmaService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        FileSystem = new(_settingsService.ApplicationSettings.OutputPath, Array.Empty<string>(), Array.Empty<string>(),
            _settingsService.ApplicationSettings.ConcurrentFileLoads, false, true);
    }

    public void Reset()
    {
        FileSystem.Clear();
    }

    public List<string>? GetArmAPBOs()
    {
        var arma3 = Path.Join(_settingsService.ApplicationSettings.ArmA3Path, "arma3.exe");
        if (!File.Exists(arma3))
            return null; // arma not found.

        List<string> pboFolders = new();
        Stack<string> checkStack = new();
        checkStack.Push(_settingsService.ApplicationSettings.ArmA3Path);

        foreach (var dir in Directory.GetDirectories(Path.Join(_settingsService.ApplicationSettings.ArmA3Path, "!Workshop")))
        {
            var name = Path.GetFileName(dir);
            if (_settingsService.ApplicationSettings.Mods.Any(x => x == name))
                checkStack.Push(dir);
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

        Log.Information("Found {punchcard} PBOs to parse.", pboPaths.Count);

        return pboPaths;
    }

    public void ReadPBO(string pboPath)
    {
        Log.Information("Reading {name}", Path.GetFileName(pboPath));

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
                    var rootedPath = file.FileName.CombineWithPrefix(root, _settingsService.ApplicationSettings.OutputPath, true);

                    var res = FileSystem.AddFile(rootedPath, file, pboPath: pbo.PBOFilePath, parentOffset: pbo.DataOffset);

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
}
