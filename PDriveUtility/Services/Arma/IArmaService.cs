using PDriveFileSystem.FileSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDriveUtility.Services.Arma;
public interface IArmaService
{
    public MemoryFileSystem FileSystem { get; protected set; }

    public void Reset();
    public List<string>? GetArmAPBOs();
    public void ReadPBO(string pboPath);
}
