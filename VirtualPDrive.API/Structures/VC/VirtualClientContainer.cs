using System.Diagnostics;

using VirtualPDrive.Client;

namespace VirtualPDrive.API.Structures.VC;

public class VirtualClientContainer
{
    public VirtualClient Client { get; set; }
    public string Id { get; set; }
    public bool Loaded { get; set; }

    public bool Errored { get; set; }
    public bool Shutdown { get; set; }
    public Stack<string> MessageStack { get; set; } = new Stack<string>();
}
