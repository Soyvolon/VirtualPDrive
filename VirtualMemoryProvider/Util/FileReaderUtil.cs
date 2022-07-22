using BIS.Core.Config;

using MemoryFS.FileSystem;

using Serilog;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMemoryProvider.Util;
public class FileReaderUtil
{
    private class State
    {
        public Guid Key { get; set; }
    }

    private readonly PriorityQueue<Task, int> _derapQueue;
    private readonly ConcurrentDictionary<Guid, Task> _activeRunners;
    private readonly int _maxRunners;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _mmfLocks = new();

    private Task? queueRunner;

    public bool Running { get; set; }

    public FileReaderUtil(int runners)
    {
        _derapQueue = new();
        _activeRunners = new();
        _maxRunners = runners;

        Running = false;
    }

    public Task EnqueueAsync(MemoryFile file, bool priority, Action<byte[]> onComplete)
    {
        if (!string.IsNullOrWhiteSpace(file.SrcPath))
        {
            // Setup the lock if needed.
            if (!_mmfLocks.TryGetValue(file.SrcPath, out var _lock))
            {
                _lock = new(1, 1);
                _mmfLocks[file.SrcPath] = _lock;
            }
        }

        var task = new Task(async (x) =>
        {
            var res = await ReadAndProcess(file);
            onComplete.Invoke(res);

            _ = _activeRunners.TryRemove((x as State)!.Key, out _);
        }, new State());

        _derapQueue.Enqueue(task, priority ? 1 : 0);

        StartIfNotStarted();

        return task.WaitAsync(Timeout.InfiniteTimeSpan);
    }

    private async Task<byte[]> ReadAndProcess(MemoryFile file)
    {
        MemoryMappedFile? mmf = null;
        string path = file.SrcPath ?? "";

        // Get the stream to read from.
        Stream? stream = null;
        try
        {
            if (file.IsFromPBO)
            {
                if (_mmfLocks.TryGetValue(file.SrcPath, out var _lock))
                {
                    if (await _lock.WaitAsync(TimeSpan.FromSeconds(5)))
                    {
                        try
                        {
                            mmf = GetMMF(file);
                            stream = mmf.CreateViewStream(file.DataOffset, file.PboDataSize, MemoryMappedFileAccess.Read);
                        }
                        finally
                        {
                            _lock.Release();
                        }
                    }
                    else
                    {
                        Log.Warning("Failed to access file - lock expired.");
                    }
                }
                else
                {
                    Log.Warning("Failed to get lock for {path}", file.SrcPath);
                }
            }
            else if (!string.IsNullOrWhiteSpace(file.SrcPath))
            {
                stream = new FileStream(file.SrcPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            
            if (stream is null)
            {
                return Array.Empty<byte>();
            }
        }
        catch (Exception ex)
        {
            Log.Warning("Error when opening stream for {path} {file}: {error}", path, file.Name, ex);
            return Array.Empty<byte>();
        }
        finally
        {
            mmf?.Dispose();
        }

        try
        {
            // Get the output data.
            byte[] output = file.Extension switch
            {
                ".bin" => await ProcessBinAsync(stream, file),
                _ => (byte[])await ProcessDefaultAsync(stream),
            };

            Log.Debug("Loaded {path} {file} with {size} bytes. Items remaining in queue: {remains}", 
                path, file.Name, output.Length, _derapQueue.Count);

            return output;
        }
        catch (Exception ex)
        {
            Log.Warning("Error when processing {path} {file}: {error}", path, file.Name, ex);
            return Array.Empty<byte>();
        }
        finally
        {
            // Cleanup
            await stream.DisposeAsync();
            mmf?.Dispose();
        }
    }

    private Task<byte[]> ProcessBinAsync(Stream stream, MemoryFile file)
    {
        // Get the file data
        var fileData = new ParamFile(stream);
        var fileString = fileData.ToString();
        var data = Encoding.ASCII.GetBytes(fileString);

        // Adjust the name
        file.ChangeExtension(".cpp");

        return Task.FromResult(data);
    }

    private async Task<byte[]> ProcessDefaultAsync(Stream stream)
    {
        List<byte> fileData = new();
        int readRes = -1;
        while (readRes != 0)
        {
            byte[] buffer = new byte[4 * 1024];
            readRes = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (readRes != 0)
            {
                for (int i = 0; i < readRes; i++)
                    fileData.Add(buffer[i]);
            }
        }

        return fileData.ToArray();
    }

    private void StartIfNotStarted()
    {
        if (!Running)
        {
            if (_derapQueue.TryPeek(out _, out _))
            {
                Running = true;

                queueRunner = Task.Run(async () =>
                {
                    while (_derapQueue.TryDequeue(out var next, out _))
                    {
                        if (next is null)
                            continue;

                        // Make sure we arent running too many at a time.
                        while (_activeRunners.Count >= _maxRunners)
                            await Task.Delay(TimeSpan.FromSeconds(0.25));

                        Guid id;
                        do
                        {
                            id = Guid.NewGuid();
                        } while (_activeRunners.ContainsKey(id));

                        // Save to the active runners dict.
                        (next.AsyncState as State)!.Key = id;
                        _activeRunners[id] = next;

                        // Start the task.
                        next.Start();
                    }

                    // Handle shutdown.
                    try
                    {
                        queueRunner?.Dispose();
                    }
                    catch { /* dispose ourselves */ }
                    finally
                    {
                        Running = false;
                        queueRunner = null;
                    }
                });
            }
        }
    }

    public async Task WaitForEmptyQueueAsync(CancellationToken? cancellation = null)
    {
        if (cancellation is null)
            cancellation = new CancellationTokenSource().Token;

        cancellation.Value.ThrowIfCancellationRequested();

        while (_derapQueue.Count > 0)
            await Task.Delay(TimeSpan.FromSeconds(0.25), cancellation.Value);

        cancellation.Value.ThrowIfCancellationRequested();

        while (_activeRunners.Count > 0)
            await Task.Delay(TimeSpan.FromSeconds(0.25), cancellation.Value);
    }

    internal MemoryMappedFile GetMMF(MemoryFile file)
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
}
