using PDriveUtility.Services.Arma;
using PDriveUtility.Services.Settings;

using System;
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
            _armaService.FileSystem.DirectoryExists()
        }
    }

    public void LoadDirectory(string dirPath)
    {
        throw new NotImplementedException();
    }

    public string GetLocalPath(string absolutePath)
    {
        throw new NotImplementedException();

    }

    public string GetAbsolutePath(string localPath)
    {
        throw new NotImplementedException();

    }
}
