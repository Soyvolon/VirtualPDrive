using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualPDrive.Client
{
    public class VirtualClientSettings
    {
        public string ArmAPath { get; set; } = "";
        public string[] ModsFilter { get; set; } = Array.Empty<string>();
        public bool NoMods { get; set; } = false;
        public string OutputPath { get; set; } = "output";
        public string? Local { get; set; } = null;
        public string[] ReadableExtensions { get; set; } = Array.Empty<string>();
        public string[] Whitelist { get; set; } = Array.Empty<string>();
        public bool PreLoad { get; set; } = false;
        public int InitRunners { get; set; } = 2;
    }
}