// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace MemoryFS
{
    public class MemoryProviderOptions
    {
        public string OutputRoot { get; set; }
        public string VirtRoot { get; set; }
        public bool EnableNotifications { get; set; }
        public bool DenyDeletes { get; set; }

        // Memory File System tools
        public string[] ReadableExtensions { get; set; } = Array.Empty<string>();
        public string[] PreloadWhitelist { get; set; } = Array.Empty<string>();
        public int InitRunners { get; set; } = 2;
    }
}
