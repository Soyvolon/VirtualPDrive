using BIS.PBO;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VirtualMemoryProvider.Util;

namespace MemoryFS.FileSystem;
public class MemoryFileSystem : IDisposable
{
    internal FileReaderUtil FileReader { get; private set; }

    private readonly HashSet<string> _readableExtensions;
    private readonly HashSet<string> _whitelistName;

    public readonly string _rootPath = "";

    public MemoryDirectory Root { get; private set; }

    private bool disposed = false;

    public MemoryFileSystem(string rootPath, string[] readableExtensions, string[] whitelistName,
        int runners)
    {
        _rootPath = rootPath;

        this._readableExtensions = readableExtensions.ToHashSet();
        this._whitelistName = whitelistName.ToHashSet();

        Root = new(_rootPath);

        FileReader = new(runners);
    }

    public bool DirectoryExists(string path)
    {
        ThrowIfDisposed();
        
        return TryGetDirectory(path, out _);
    }

    public bool FileExists(string path)
    {
        ThrowIfDisposed();

        return TryGetFile(path, out _);
    }

    public bool TryGetFile(string path,
        [NotNullWhen(true)] out MemoryFile? file)
    {
        ThrowIfDisposed();

        return Root.TryGetFile(path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries), out file);
    }

    public bool TryGetDirectory(string path,
        [NotNullWhen(true)] out MemoryDirectory? dir)
    {
        ThrowIfDisposed();

        return Root.TryGetDirectory(path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries), out dir);
    }

    public MemoryFile? AddFile(string path, FileEntry? entry = null, string? srcPath = null, string? pboPath = null, int parentOffset = 0)
    {
        ThrowIfDisposed();

        var dirName = Path.GetDirectoryName(path);
        if (dirName is not null)
        {
            var dir = AddDirectory(dirName);
            if (dir is not null)
            {
                var ext = Path.GetExtension(path);
                var name = Path.GetFileName(path);

                bool read = true;
                if (_readableExtensions.Count > 0)
                    read = _readableExtensions.Contains(ext);
                if (_whitelistName.Count > 0)
                    read = _whitelistName.Contains(name);

                return dir.AddFile(entry, srcPath, pboPath, parentOffset, read, name, ext, FileReader);
            }
        }

        return null;
    }

    public MemoryDirectory? AddDirectory(string path)
    {
        ThrowIfDisposed();

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

    public async Task InitalizeFileSystemAsync()
    {
        ThrowIfDisposed();

        if (FileReader is not null)
        {
            Stack<MemoryDirectory> loadStack = new();
            loadStack.Push(Root);

            while (loadStack.TryPop(out var parent))
            {
                foreach (var item in parent.Directories)
                {
                    loadStack.Push(item);
                }

                _ = parent.InitalizeDirectory(false);
            }

            await FileReader.WaitForEmptyQueueAsync();
        }
    }

    public async Task InitalizeFileAsync(string path)
    {
        ThrowIfDisposed();

        if (TryGetFile(path, out var file))
            await InitalizeFileAsync(file);
    }

    private async Task InitalizeFileAsync(MemoryFile file)
    {
        ThrowIfDisposed();

        await file.WaitForInitAsync();
    }

#nullable disable
    private void ThrowIfDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(MemoryFileSystem), "This memory file system is disposed.");
    }

    public void Dispose()
    {
        disposed = true;

        FileReader.Dispose();
        Root.Dispose();

        Root = null;
        FileReader = null;
    }
#nullable enable
}
