// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Serilog;
using System;

namespace VirtualMemoryProvider
{
    public class Program
    {
        private enum ReturnCode
        {
            Success = 0,
            InvalidArguments = 1,
            GeneralException = 2,
        }

        private static void Run()
        {
            MemoryProvider provider;
            try
            {
                provider = new MemoryProvider(new());
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to create SimpleProvider.");
                throw;
            }

            Log.Information("Starting provider");

            if (!provider.StartVirtualization())
            {
                Log.Error("Could not start provider.");
                Environment.Exit(1);
            }

            Console.WriteLine("Provider is running.  Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
