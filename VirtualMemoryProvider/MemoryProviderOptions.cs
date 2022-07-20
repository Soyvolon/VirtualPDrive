// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace VirtualMemoryProvider
{
    public class MemoryProviderOptions
    {
        public string OutputRoot { get; set; }
        public string VirtRoot { get; set; }

        public bool EnableNotifications { get; set; }

        public bool DenyDeletes { get; set; }
    }
}
