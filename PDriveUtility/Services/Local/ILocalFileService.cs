using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDriveUtility.Services.Local;
public interface ILocalFileService
{
    public List<string>? GetLocalDirectories();
    public void LoadDirectory(string dirPath);
    public string GetLocalPath(string absolutePath);
    public string GetAbsolutePath(string localPath);
}
