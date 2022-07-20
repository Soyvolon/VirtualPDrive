using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualPDrive.PBO;
public class PBOFile
{
    public string RelativePath { get; init; }
    public string FileName { get; init; }

    public PBOFile(string path)
    {
        RelativePath = path;
        FileName = Path.GetFileName(path);
    }
}
