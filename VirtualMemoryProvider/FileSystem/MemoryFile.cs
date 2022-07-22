using Serilog;

using BIS.PBO;

using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VirtualMemoryProvider.Util;

namespace MemoryFS.FileSystem;
public class MemoryFile : IMemoryItem
{
    public string Name { get; set; }
    public string Extension { get; set; }

    public string SrcPath { get; init; }
    public bool IsFromPBO { get; init; }
    public bool AllowRead { get; init; }
    public int DataOffset { get; init; }
    public int PboDataSize { get; init; }

    internal byte[] _fileData = Array.Empty<byte>();
    internal bool _initalized = false;
    internal bool _initalizing = false;

    private FileReaderUtil _fileReader;

    private bool disposed;

    public MemoryFile(FileEntry? entry, string? srcPath, string? pboPath, int parentOffset, bool allowRead, string name, string extension, FileReaderUtil fileReader)
    {
        SrcPath = srcPath ?? "";
        AllowRead = allowRead;

        Name = name;
        Extension = extension;

        if (srcPath is null && entry is not null)
        {
            SrcPath = pboPath ?? "";
            DataOffset = parentOffset + entry.StartOffset;
            PboDataSize = entry.DataSize;
            IsFromPBO = true;
        }

        _fileReader = fileReader;
    }

    internal void Initalize(bool priority = false)
    {
        ThrowIfDisposed();

        if (!AllowRead)
            return;

        _initalizing = true;
        _fileReader.Enqueue(this, priority);
    }

    internal async Task<bool> WaitForInitAsync(bool priority = false)
    {
        ThrowIfDisposed();

        if (_initalized)
            return true;

        if (!_initalizing)
            Initalize(priority);

        do
        {
            await Task.Delay(TimeSpan.FromSeconds(0.25));
        } while (_initalizing);

        return _initalized;
    }

    public MemoryStream GetStream()
    {
        ThrowIfDisposed();

        return new(_fileData);
    }

    public void ChangeExtension(string newExtension)
    {
        ThrowIfDisposed();

        if (!newExtension.StartsWith('.'))
            newExtension = "." + newExtension;

        Extension = newExtension;
        Name = Path.GetFileNameWithoutExtension(Name) + newExtension;
    }

    public long GetSize()
    {
        ThrowIfDisposed();

        // if we dont want to read this file, set this to 0.
        // Any open attempts will not call the API.
        if (!AllowRead)
            return 0;

        // If its not initalized, just return the 
        // pbo data size - all we need is a value
        // larger than 0 to trigger the hydrate file API.
        if (!_initalized)
            return PboDataSize;

        // If we have initalized, return the actual
        // data length for this file.
        return _fileData.Length;
    }

    public MemoryStream? GetDataStream()
    {
        ThrowIfDisposed();

        if (!_initalized)
        {
            var res = WaitForInitAsync(true).GetAwaiter().GetResult();
            if (!res)
                return null;
        }

        return new MemoryStream(_fileData);
    }
    private void ThrowIfDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(MemoryFileSystem), "This memory file system is disposed.");
    }

    public void Dispose()
    {
        _fileData = null;
        _fileReader = null;
    }
}
