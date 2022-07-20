using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMemoryProvider.FileSystem;
public class MemoryFileSystem : IDisposable
{
    public readonly string _rootPath = "";
    private bool disposedValue;

    public MemoryDirectory Root { get; private set; }

    public MemoryFileSystem(string rootPath)
    {
        _rootPath = rootPath;
        Root = new(_rootPath);
    }

    public bool DirectoryExists(string path)
        => TryGetDirectory(path, out _);

    public bool FileExists(string path)
        => TryGetFile(path, out _);

    public bool TryGetFile(string path,
        [NotNullWhen(true)] out MemoryFile? file)
        => Root.TryGetFile(path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries), out file);

    public bool TryGetDirectory(string path,
        [NotNullWhen(true)] out MemoryDirectory? dir)
        => Root.TryGetDirectory(path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries), out dir);

    public MemoryFile? AddFile(string path)
    {
        var dirName = Path.GetDirectoryName(path);
        if (dirName is not null)
        {
            var dir = AddDirectory(dirName);
            if (dir is not null)
            {
                return dir.AddFile(Path.GetFileName(path), Path.GetExtension(path));
            }
        }

        return null;
    }

    public MemoryDirectory? AddDirectory(string path)
    {
        var dirs = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        var parent = Root;
        foreach (var dir in dirs)
        {
            if (parent is not null)
                parent = parent.AddDirectory(dir);
            else break;
        }

        return parent;
    }

#nullable disable
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {

            }

            Root = null;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
#nullable enable
}
