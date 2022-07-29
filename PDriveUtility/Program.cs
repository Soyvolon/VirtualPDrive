using McMaster.Extensions.CommandLineUtils;

using PDriveUtility.Forms.Init;
using PDriveUtility.Structures.Init;

namespace PDriveUtility.GUI;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        CommandLineApplication.Execute<ConsoleStartup>(args);
    }
}