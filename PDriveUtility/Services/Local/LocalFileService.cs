using PDriveUtility.Services.Arma;
using PDriveUtility.Services.Settings;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDriveUtility.Services.Local;
public class LocalFileService : ILocalFileService
{
    private readonly ISettingsService _settingsService;
    private readonly IArmaService _armaService;



    public LocalFileService(ISettingsService settingsService, IArmaService armaService)
    {
        _settingsService = settingsService;
        _armaService = armaService;
    }

    public List<string>? GetLocalDirectories()
    {
        var outputDir = _settingsService.ApplicationSettings.OutputPath;
        if (!Directory.Exists(outputDir))
            return null;

        List<string> output = new();
        var dirs = Directory.GetDirectories(outputDir);
        foreach (var dir in dirs)
        {
            var local = GetLocalPath(dir);
            if (_armaService.FileSystem.DirectoryExists(local, false))
            {
                output.Add(dir);
            }
        }

        return output;
    }

    public void LoadDirectory(string dirPath)
    {
        // throw new NotImplementedException();
    }

    public string GetLocalPath(string absolutePath)
    {
        return Path.GetRelativePath(_settingsService.ApplicationSettings.OutputPath, absolutePath);
    }

    public string GetAbsolutePath(string localPath)
    {
        return Path.Join(_settingsService.ApplicationSettings.OutputPath, localPath);
    }
}
