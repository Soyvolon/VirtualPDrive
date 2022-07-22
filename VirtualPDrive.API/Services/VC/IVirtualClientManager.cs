﻿using VirtualPDrive.API.Structures.VC;
using VirtualPDrive.Client;

namespace VirtualPDrive.API.Services.VC;

public interface IVirtualClientManager
{
    public VirtualClientContainer CreateVirtualClient(VirtualClientSettings settings, bool randomOutput, bool generatedRandomOutputFolder = false);
    public VirtualClientContainer? GetVirtualClient(string id);
    public VirtualClientContainer? DestroyVirtualClient(string id);
}
