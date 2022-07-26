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
public class FileReaderUtil : IDisposable
{
    private class State
    {
        public Guid Key { get; set; }
    }

    private readonly ConcurrentQueue<MemoryFile> _derapQueue;
    private readonly ConcurrentDictionary<Guid, Task> _activeRunners;
    private readonly int _maxRunners;

    private Task? queueRunner;
    private readonly CancellationTokenSource _cancelSource;
    private readonly CancellationToken _cancellationToken;

    public bool Running { get; set; }
    private bool _disposed = false;

    public FileReaderUtil(int runners)
    {
        _derapQueue = new();
        _activeRunners = new();
        _maxRunners = runners;

        _cancelSource = new CancellationTokenSource();
        _cancellationToken = _cancelSource.Token;

        Running = false;
    }

    public void Enqueue(MemoryFile file, bool priority)
    {
        if (_disposed)
            return;

        _cancellationToken.ThrowIfCancellationRequested();

        if (!string.IsNullOrWhiteSpace(file.SrcPath))
        {
            if (MMFUtil.Active is null)
                MMFUtil.Active = new();

            // Setup the lock if needed.
            if (!MMFUtil.Active.Locks.TryGetValue(file.SrcPath, out _))
            {
                var _lock = new SemaphoreSlim(1, 1);
                MMFUtil.Active.Locks[file.SrcPath] = _lock;
            }
        }

        _cancellationToken.ThrowIfCancellationRequested();

        if (!priority)
            _derapQueue.Enqueue(file);
        else
            RunItem(file);

        StartIfNotStarted();
    }

    private async Task ReadAndProcess(MemoryFile file)
    {
        if (_disposed)
            return;

        if (MMFUtil.Active is null)
        {
            file._initalizing = false;
            return;
        }

        MemoryMappedFile? mmf = null;
        string path = file.SrcPath ?? "";

        _cancellationToken.ThrowIfCancellationRequested();

        // Get the stream to read from.
        Stream? stream = null;
        try
        {
            if (file.IsFromPBO)
            {
                if (MMFUtil.Active.Locks.TryGetValue(file.SrcPath!, out var _lock))
                {
                    if (await _lock.WaitAsync(TimeSpan.FromSeconds(30), _cancellationToken))
                    {
                        try
                        {
                            mmf = MMFUtil.Active.GetMMF(file);
                            stream = mmf.CreateViewStream(file.DataOffset, file.PboDataSize, MemoryMappedFileAccess.Read);
                        }
                        finally
                        {
                            _lock.Release();

                            _cancellationToken.ThrowIfCancellationRequested();
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
                return;
            }

            _cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException) { return; }
        catch (Exception ex)
        {
            Log.Warning("Error when opening stream for {path} {file}: {error}", path, file.Name, ex);
            return;
        }
        finally
        {
            mmf?.Dispose();
            file._initalizing = false;
        }

        try
        {
            _cancellationToken.ThrowIfCancellationRequested();

            // Get the output data.
            byte[] output = file.Extension switch
            {
                ".bin" => await ProcessBinAsync(stream, file),
                _ => (byte[])await ProcessDefaultAsync(stream),
            };

            _cancellationToken.ThrowIfCancellationRequested();

            Log.Debug("Loaded {path} {file} with {size} bytes. Items remaining in queue: {remains}", 
                path, file.Name, output.Length, _derapQueue.Count);

            file._initalized = true;
            file._fileData = output;
        }
        catch (OperationCanceledException) { return; }
        catch (Exception ex)
        {
            Log.Warning("Error when processing {path} {file}: {error}", path, file.Name, ex);
            return;
        }
        finally
        {
            // Cleanup
            await stream.DisposeAsync();
            mmf?.Dispose();
            file._initalizing = false;
        }
    }

    private Task<byte[]> ProcessBinAsync(Stream stream, MemoryFile file)
    {
        if (_disposed)
            return Task.FromResult(Array.Empty<byte>());

        // Get the file data
        var fileData = new ParamFile(stream);

        _cancellationToken.ThrowIfCancellationRequested();

        var fileString = fileData.ToString();

        _cancellationToken.ThrowIfCancellationRequested();

        var data = Encoding.ASCII.GetBytes(fileString);

        _cancellationToken.ThrowIfCancellationRequested();

        // Adjust the name
        file.ChangeExtension(".cpp");

        return Task.FromResult(data);
    }

    private async Task<byte[]> ProcessDefaultAsync(Stream stream)
    {
        if (_disposed)
            return Array.Empty<byte>();

        _cancellationToken.ThrowIfCancellationRequested();

        List<byte> fileData = new();
        int readRes = -1;
        while (readRes != 0)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            byte[] buffer = new byte[4 * 1024];
            readRes = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (readRes != 0)
            {
                for (int i = 0; i < readRes; i++)
                    fileData.Add(buffer[i]);
            }
        }

        _cancellationToken.ThrowIfCancellationRequested();

        return fileData.ToArray();
    }

    private void StartIfNotStarted()
    {
        if (_disposed)
            return;

        _cancellationToken.ThrowIfCancellationRequested();

        if (!Running)
        {
            if (_derapQueue.TryPeek(out _))
            {
                _cancellationToken.ThrowIfCancellationRequested();

                Running = true;

                queueRunner = new Task(async (state) =>
                {
                    while (_derapQueue.TryDequeue(out var next))
                    {
                        if (_cancellationToken.IsCancellationRequested)
                            break;

                        if (next is null)
                            continue;

                        // Make sure we arent running too many at a time.
                        while (_activeRunners.Count >= _maxRunners)
                            await Task.Delay(TimeSpan.FromSeconds(0.25));

                        RunItem(next);
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
                }, null, _cancellationToken, TaskCreationOptions.LongRunning);

                queueRunner.Start();
            }
        }
    }

    private void RunItem(MemoryFile next)
    {
        if (_disposed)
            return;

        Guid id;
        do
        {
            id = Guid.NewGuid();
        } while (_activeRunners.ContainsKey(id));

        var task = new Task(async (x) =>
        {
            await ReadAndProcess(next);

            _ = _activeRunners.TryRemove(id, out _);
        }, id, _cancellationToken);

        _activeRunners[id] = task;

        // May have been cancelled already.
        if (!task.IsCompleted)
            // Start the task.
            task.Start();
    }

    public async Task WaitForEmptyQueueAsync(CancellationToken? cancellation = null)
    {
        if (_disposed)
            return;

        if (cancellation is null)
            cancellation = _cancellationToken;

        cancellation.Value.ThrowIfCancellationRequested();

        while (_derapQueue.Count > 0)
            await Task.Delay(TimeSpan.FromSeconds(0.25), cancellation.Value);

        cancellation.Value.ThrowIfCancellationRequested();

        while (_activeRunners.Count > 0)
            await Task.Delay(TimeSpan.FromSeconds(0.25), cancellation.Value);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _cancelSource.Cancel();
        _cancelSource.Dispose();

        // We wont make new runners if this is true.
        Running = true;
        _derapQueue.Clear();
        _activeRunners.Clear();
        queueRunner = null;
    }
}
