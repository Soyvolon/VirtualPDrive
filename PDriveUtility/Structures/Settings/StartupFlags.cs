using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDriveUtility.Structures.Settings;
public class StartupFlags
{
    public bool Arma3NotFound { get; set; } = false;
    public bool OutputPathNotFound { get; set; } = false;

    public bool SkipArma3 { get; set; } = false;
    public bool SkipOutput { get; set; } = false;
}
