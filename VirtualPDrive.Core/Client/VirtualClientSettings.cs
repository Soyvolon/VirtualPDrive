using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualPDrive.Core.Client
{
    public class VirtualClientSettings
    {
        public string ArmAPath { get; set; } = "";
        public string[] ModsFilter { get; set; } = Array.Empty<string>();
        public bool NoMods { get; set; } = false;
        public string OutputPath { get; set; } = "output";
        public string Local { get; set; } = null;
    }
}