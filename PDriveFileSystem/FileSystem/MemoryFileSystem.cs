using BIS.PBO;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PDriveFileSystem.Util;

namespace PDriveFileSystem.FileSystem;
public class MemoryFileSystem : IDisposable
{
    internal static FileReaderUtil FileReader { get; private set; }

    private readonly HashSet<string> _readableExtensions;
    private readonly HashSet<string> _whitelistName;
    private readonly bool _skipWhitelist;

    public readonly string _rootPath = "";

    public HashSet<string> Directories { get; private set; } = new();
    public ConcurrentDictionary<string, MemoryFile> Files { get; private set; } = new();
    public ConcurrentDictionary<string, HashSet<(string, bool)>> ChildMap { get; private set; } = new();
    public ConcurrentDictionary<string, string> LowercaseMap { get; private set; } = new();

    private bool disposed = false;

    public MemoryFileSystem(string rootPath, string[] readableExtensions, string[] whitelistName,
        int runners, bool local, bool skipWhitelist = false, bool skipClean = false)
    {
        _rootPath = rootPath;

        this._readableExtensions = readableExtensions.ToHashSet();
        this._whitelistName = whitelistName.ToHashSet();
        this._skipWhitelist = skipWhitelist;

        if (FileReader is null)
            FileReader = new(runners);

        // add root dir.
        Directories.Add("");
        if (!ChildMap.ContainsKey(""))
            ChildMap[""] = new HashSet<(string, bool)>();
        LowercaseMap[""] = "";

        if (!local && !skipClean)
        {
            foreach (var file in Directory.GetFiles(rootPath))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {

                }
            }

            foreach (var dir in Directory.GetDirectories(rootPath))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }

    public bool DirectoryExists(string path, bool caseSensitive)
    {
        ThrowIfDisposed();
        
        return TryGetDirectory(path, caseSensitive, out _);
    }

    public bool FileExists(string path, bool caseSensitive)
    {
        ThrowIfDisposed();

        return TryGetFile(path, caseSensitive, out _);
    }

    public bool TryGetFile(string path, bool caseSensitive,
        [NotNullWhen(true)] out MemoryFile? file)
    {
        ThrowIfDisposed();

        var actualPath = GetPath(path, caseSensitive);

        if (actualPath is null)
        {
            file = null;
            return false;
        }

        return Files.TryGetValue(actualPath, out file);
    }

    public bool TryGetDirectory(string path, bool caseSensitive,
        [NotNullWhen(true)] out string? dir)
    {
        ThrowIfDisposed();

        dir = Path.GetFileName(path);

        var actualPath = GetPath(path, caseSensitive);

        if (actualPath is null)
        {
            dir = null;
            return false;
        }

        return Directories.Contains(actualPath);
    }

    public HashSet<(string, bool)> GetChildEntries(string path, bool caseSensitive)
    {
        var actualPath = GetPath(path, caseSensitive);

        if (actualPath is null)
            return new();

        if (ChildMap.TryGetValue(actualPath, out var data))
            return data;

        return new();
    }

    public MemoryFile? AddFile(string path, FileEntry? entry = null, string? srcPath = null, string? pboPath = null, int parentOffset = 0)
    {
        ThrowIfDisposed();

        if (Files.TryGetValue(path, out _))
            return null;

        var ext = Path.GetExtension(path);
        var name = Path.GetFileName(path);

        bool read = _skipWhitelist
            || _readableExtensions.Contains(ext)
            || _whitelistName.Contains(name);

        var file = new MemoryFile(entry, srcPath, pboPath, parentOffset, read, name, ext, path, _rootPath, this);

        Files[path] = file;
        var dir = Path.GetDirectoryName(path) ?? "";
        dir = AddDirectory(dir);

        if (dir is not null)
        {
            if (ChildMap.TryGetValue(dir, out var objects))
                objects.Add((path, false));
            else
                ChildMap[dir] = new HashSet<(string, bool)> { (path, false) };
        }

        LowercaseMap[path.ToLower()] = path;

        return file;
    }

    public string? AddDirectory(string path)
    {
        ThrowIfDisposed();

        if (path == "")
        {
            return path;
        }

        if (!Path.HasExtension(path))
        {
            var parts = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < parts.Length; i++)
            {
                var dir = string.Join(Path.DirectorySeparatorChar, parts[..(i + 1)]);

                Directories.Add(dir); 
                
                var parent = Path.GetDirectoryName(dir) ?? "";
                if (ChildMap.TryGetValue(parent, out var objects))
                {
                    objects.Add((dir, true));
                }
                else
                {
                    ChildMap[parent] = new HashSet<(string, bool)> { (dir, true) };
                }

                LowercaseMap[dir.ToLower()] = dir;
            }

            return path;
        }

        return null;
    }

    public async Task InitalizeFileSystemAsync()
    {
        ThrowIfDisposed();

        foreach (var file in Files.Values)
            await InitalizeFileAsync(file, false);

        await FileReader.WaitForEmptyQueueAsync();
    }

    public async Task InitalizeFileAsync(string path)
    {
        ThrowIfDisposed();

        var actualPath = GetPath(path, false);

        if (actualPath is null)
            return;

        if (Files.TryGetValue(actualPath, out var file))
            await InitalizeFileAsync(file);
    }

    private async Task InitalizeFileAsync(MemoryFile file, bool wait = true)
    {
        ThrowIfDisposed();

        if (wait)
        {
            await file.WaitForInitAsync();
        }
        else
        {
            file.Initalize();
        }
    }

#nullable disable
    private void ThrowIfDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(MemoryFileSystem), "This memory file system is disposed.");
    }

    private string? GetPath(string path, bool caseSensitive)
    {
        string actualPath;
        if (caseSensitive)
        {
            actualPath = path;
        }
        else
        {
            var tmp = path.ToLower();
            if (!LowercaseMap.TryGetValue(tmp, out actualPath!))
            {
                return null;
            }
        }

        return actualPath;
    }

    public void Clear()
    {
        foreach (var file in Files.Values)
            file.Dispose();

        Files.Clear();
        Directories.Clear();
        LowercaseMap.Clear();
        ChildMap.Clear();
    }

    public void Dispose()
    {
        disposed = true;

        FileReader?.Dispose();
        FileReader = null;

        foreach (var file in Files.Values)
            file.Dispose();
        Files = null;
        Directories = null;
        LowercaseMap = null;
        ChildMap = null;
    }
#nullable enable
}
