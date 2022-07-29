using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDriveUtility.Structures.Settings;
public class ApplicationSettings
{
    public string ArmA3Path { get; set; } = "";
    public string OutputPath { get; set; } = @"P:\";
    public List<string> Mods { get; set; } = new();
    public int ConcurrentFileLoads { get; set; }
}
