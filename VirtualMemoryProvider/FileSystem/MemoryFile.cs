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

    private byte[] _fileData = Array.Empty<byte>();
    private bool _initalized = false;

    public MemoryFile(FileEntry? entry, string? srcPath, bool allowRead, string name, string extension)
    {
        SrcPath = srcPath ?? "";
        AllowRead = allowRead;

        Name = name;
        Extension = extension;

        if (srcPath is null && entry is not null)
        {
            SrcPath = entry.Parent.PBOFilePath;
            DataOffset = entry.Parent.DataOffset + entry.StartOffset;
            PboDataSize = entry.DataSize;
            IsFromPBO = true;
        }
    }

    internal Task InitalizeAsync(bool priority = false)
    {
        if (!AllowRead || MemoryFileSystem.FileReader is null)
            return Task.CompletedTask;

        return MemoryFileSystem.FileReader.EnqueueAsync(this, priority, (x) =>
        {
            _fileData = x;
            _initalized = true;
        });
    }

    public MemoryStream GetStream()
        => new(_fileData);

    public void ChangeExtension(string newExtension)
    {
        if (!newExtension.StartsWith('.'))
            newExtension = "." + newExtension;

        Extension = newExtension;
        Name = Path.GetFileNameWithoutExtension(Name) + newExtension;
    }

    public long GetSize()
    {
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

    public MemoryStream GetDataStream()
    {
        if (!_initalized)
            InitalizeAsync(true).Wait();

        return new MemoryStream(_fileData);
    }
}
