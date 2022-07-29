using Serilog.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDriveUtility.Structures.Settings;
public class LoggingSettings
{
    public bool WriteToConsole { get; set; } = true;
    public LogEventLevel ConsoleLevel { get; set; } = LogEventLevel.Information;

    public bool WriteToFile { get; set; } = true;
    public string LogFolderPath { get; set; } = "Logs";
    public int RollingFiles { get; set; } = 4;
    public LogEventLevel FileLevel { get; set; } = LogEventLevel.Debug;
}
