using BIS.PBO;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VirtualMemoryProvider.Util;

namespace MemoryFS.FileSystem;
public class MemoryDirectory : IMemoryItem
{
    public string Name { get; set; }
    public List<MemoryDirectory> Directories { get; set; } = new();
    public HashSet<MemoryFile> Files { get; set; } = new();

    public MemoryDirectory(string name)
    {
        Name = name;
    }

    public IEnumerable<IMemoryItem> GetEntries()
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
            file = Files.FirstOrDefault(x => x.Name.Equals(path[0]));
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
            if (localDir.Name.Equals(path[0]))
                return localDir.TryGetDirectory(path.Length == 1 ? Array.Empty<string>() : path.Skip(1).ToArray(), out dir);
        }

        dir = null;
        return false;
    }

    public MemoryFile AddFile(FileEntry? entry, string? srcPath, string? pboPath, int parentOffset, bool allowRead, string name, string ext)
    {
        var file = new MemoryFile(entry, srcPath, pboPath, parentOffset, allowRead, name, ext);
        if(!Files.Add(file))
        {
            if(Files.TryGetValue(file, out var actual))
            {
                file = actual;
            }
        }

        return file;
    }

    public MemoryDirectory AddDirectory(string name)
    {
        if (name == Name)
            return this;

        var dirSearch = Directories.FirstOrDefault(x => x.Name.Equals(name));
        if (dirSearch is not null)
            return dirSearch;

        var dir = new MemoryDirectory(name);
        Directories.Add(dir);

        return dir;
    }

    internal async Task InitalizeDirectory(bool wait)
    {
        foreach (var file in Files)
            file.Initalize();

        if (wait)
        {
            foreach (var file in Files)
                await file.WaitForInitAsync();
        }
    }

    public void Dispose()
    {
        foreach (var file in Files)
            file.Dispose();

        foreach (var dir in Directories)
            dir.Dispose();

        Files = null;
        Directories = null;
    }
}
