using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMemoryProvider.FileSystem;
public class MemoryDirectory : MemoryItem
{
    public List<MemoryDirectory> Directories { get; set; } = new();
    public HashSet<MemoryFile> Files { get; set; } = new();

    public MemoryDirectory(string name)
    {
        Name = name;
    }

    public IEnumerable<MemoryItem> GetEntries()
    {
        foreach (var dir in Directories)
            yield return dir;

        foreach (var file in Files)
            yield return file;
    }

    public bool TryGetFile(string[] path,
        [NotNullWhen(true)] out MemoryFile? file)
    {
        if (path.Length == 1)
        {
            file = Files.FirstOrDefault(x => x.Name.Equals(path[0], StringComparison.OrdinalIgnoreCase));
            return file is not null;
        }

        if (TryGetDirectory(path[..^1], out var dir))
            return dir.TryGetFile(path[^1..], out file);

        file = null;
        return false;
    }

    public bool TryGetDirectory(string[] path,
        [NotNullWhen(true)] out MemoryDirectory? dir)
    {
        if (path.Length == 0)
        {
            dir = this;
            return true;
        }

        foreach(var localDir in Directories)
        {
            if (localDir.Name.Equals(path[0], StringComparison.OrdinalIgnoreCase))
                return localDir.TryGetDirectory(path.Length == 1 ? Array.Empty<string>() : path.Skip(1).ToArray(), out dir);
        }

        dir = null;
        return false;
    }

    public MemoryFile AddFile(string name, string extension)
    {
        var file = new MemoryFile(name, extension);
        Files.Add(file);

        return file;
    }

    public MemoryDirectory AddDirectory(string name)
    {
        if (name == Name)
            return this;

        var dirSearch = Directories.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (dirSearch is not null)
            return dirSearch;

        var dir = new MemoryDirectory(name);
        Directories.Add(dir);

        return dir;
    }
}
