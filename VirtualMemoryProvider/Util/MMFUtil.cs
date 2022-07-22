using MemoryFS.FileSystem;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMemoryProvider.Util;
public class MMFUtil : IDisposable
{
    public static MMFUtil? Active { get; set; } = null;
    
    public ConcurrentDictionary<string, SemaphoreSlim> Locks { get; private set; } = new();

    public MemoryMappedFile GetMMF(MemoryFile file)
    {
        MemoryMappedFile mmf;
        var name = file.SrcPath.Replace(Path.DirectorySeparatorChar.ToString(), "");
        try
        {
            // TODO Make this platform compatable.
#pragma warning disable CA1416 // Validate platform compatibility
            mmf = MemoryMappedFile.OpenExisting(name);
#pragma warning restore CA1416 // Validate platform compatibility
        }
        catch
        {
            mmf = MemoryMappedFile.CreateFromFile(file.SrcPath, FileMode.Open, name);
        }

        return mmf;
    }

    public void Dispose()
    {
        foreach (var l in Locks.Values)
            l.Dispose();

        Locks = null;
    }
}
