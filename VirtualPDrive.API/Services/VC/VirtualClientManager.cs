using Serilog;

using System.Collections.Concurrent;

using VirtualMemoryProvider.Util;

using VirtualPDrive.API.Extensions;
using VirtualPDrive.API.Structures.VC;
using VirtualPDrive.Client;

namespace VirtualPDrive.API.Services.VC;

public class VirtualClientManager : IVirtualClientManager
{
    private readonly IConfiguration _configuration;

    private readonly int _cacheTime = 30;

    private HashSet<string> DestroyFolders { get; set; } = new();
    private ConcurrentDictionary<string, string[]> CurrentPaths { get; init; } = new();
    private ConcurrentDictionary<string, VirtualClientContainer> Containers { get; init; } = new();
    private ConcurrentDictionary<VirtualClient, string> ClientLookup { get; init; } = new();
    private ConcurrentDictionary<string, Timer> ErrorCacheTimers { get; set; } = new();

    public VirtualClientManager(IConfiguration configuration)
    {
        _configuration = configuration;
        _cacheTime = _configuration.GetValue("ClientErrorCacheTime", 30);
    }

    public VirtualClientContainer CreateVirtualClient(VirtualClientSettings settings, bool randomOutput, bool forceGenOutput, 
        bool generatedRandomOutputFolder = false, int tests = 0)
    {
        VirtualClient? client = null;
        string[]? clientPath = null;

        if (!forceGenOutput)
        {
            client = new VirtualClient(settings);
            clientPath = client.Settings.OutputPath.Split(Path.DirectorySeparatorChar);

            if (tests > 5)
            {
                var dir = Path.GetDirectoryName(settings.OutputPath);
                Log.Warning("Failed to generate a new non-child path in {path}", dir);
                throw new Exception("Failed to generate a new non-child path in " + dir);
            }

            foreach (var path in CurrentPaths.Values)
            {
                if (clientPath.IsChildOfOrEqualTo(path))
                {
                    if (randomOutput)
                    {
                        forceGenOutput = true;
                        tests++;
                    }
                    else
                    {
                        throw new Exception("The provided path is already a part of an existing virtual instance.");
                    }
                }
            }
        }

        if (forceGenOutput)
        {
            string newPath;
            do
            {
                string prefix = "instances";
#if DEBUG
                prefix = Path.Combine("bin", prefix);
#endif
                newPath = Path.Combine(prefix, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            } while (Directory.Exists(newPath));

            Directory.CreateDirectory(newPath);
            settings.OutputPath = newPath;

            // We want to check the URI again just in case, but it should not be a problem.
            return CreateVirtualClient(settings, randomOutput, false, true, tests);
        }

        if (client is null)
        {
            Log.Warning("Failed to create virtual client for {settings}", settings);
            throw new Exception("No client was able to be created.");
        }

        if (clientPath is null)
            clientPath = client.Settings.OutputPath.Split(Path.DirectorySeparatorChar);

        client.OnStart += Client_OnStart;
        client.OnShutdown += Client_OnShutdown;
        client.OnError += Client_OnError;

        string key;
        do {
            key = Guid.NewGuid().ToString();
        } while (Containers.ContainsKey(key));

        var container = new VirtualClientContainer()
        {
            Client = client,
            Loaded = false,
            Id = key
        };

        Containers[key] = container;
        ClientLookup[client] = key;
        CurrentPaths[key] = clientPath;

        if (generatedRandomOutputFolder)
        {
            DestroyFolders.Add(key);

            Log.Information("Created auto generated directory {path}", settings.OutputPath);
        }

        try
        {
            client.Start();
        }
        catch (Exception ex)
        {
            Task.Run(async () => await Client_OnError(client, new()
            {
                Exception = ex,
                Message = ex.Message
            }));
        }

        return container;
    }

    public VirtualClientContainer? DestroyVirtualClient(string id)
    {
        if (Containers.TryRemove(id, out var container))
        {
            if (container.Client is not null)
            {
                _ = ClientLookup.TryRemove(container.Client, out _);

                string path = container.Client.Settings.OutputPath;

                container.Client.Dispose();

                if (DestroyFolders.TryGetValue(id, out var actual))
                {
                    DestroyFolders.Remove(actual);
                    Directory.Delete(path, true);

                    Log.Information("Deleted auto generated directory {path}", actual);
                }
            }
            else
            {
                if (DestroyFolders.TryGetValue(id, out var actual))
                {
                    DestroyFolders.Remove(actual);
                    try
                    {
                        Directory.Delete(actual, true);

                        Log.Information("Deleted auto generated directory {path}", actual);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("Failed to delete auto generated directory {path}: {err}", actual, ex);
                    }
                }
            }

            _ = ErrorCacheTimers.TryRemove(id, out _);
            _ = CurrentPaths.TryRemove(id, out _);

            if (Containers.Count == 0
                && MMFUtil.Active is not null)
            {
                MMFUtil.Active.Dispose();
                MMFUtil.Active = null;
            }

            return container;
        }

        return null;
    }

    public VirtualClientContainer? GetVirtualClient(string id)
    {
        _ = Containers.TryGetValue(id, out var container);

        return container;
    }

    private void UpdateCache(string key)
    {
        if (ErrorCacheTimers.TryGetValue(key, out var timer))
        {
            timer.Change(TimeSpan.FromSeconds(_cacheTime),
                Timeout.InfiniteTimeSpan);
        }
        else
        {
            ErrorCacheTimers[key] = new((x) =>
            {
                _ = DestroyVirtualClient((string)x);
            }, key, TimeSpan.FromSeconds(_cacheTime),
                Timeout.InfiniteTimeSpan);
        }
    }

    #region Client Events

    private async Task Client_OnStart(object sender)
    {
        await Task.Run(() =>
        {
            if (sender is VirtualClient client)
            {
                if (ClientLookup.TryGetValue(client, out var key))
                {
                    if (Containers.TryGetValue(key, out var container))
                    {
                        container.Loaded = true;
                    }
                }
            }
        });
    }

    private async Task Client_OnShutdown(object sender)
    {
        await Task.Run(() =>
        {
            if (sender is VirtualClient client)
            {
                if (ClientLookup.TryGetValue(client, out var key))
                {
                    if (Containers.TryGetValue(key, out var container))
                    {
#nullable disable
                        container.Client.OnStart -= Client_OnStart;
                        container.Client.OnError -= Client_OnError;
                        container.Client.OnShutdown -= Client_OnShutdown;
                        container.Client.Dispose();

                        container.Client = null;
                        container.Loaded = false;
                        container.Errored = false;
                        container.Shutdown = true;

                        container.MessageStack.Push("Client gracefully shutdown.");

                        UpdateCache(key);

                        // Remove from current paths now as the system is already stopped.
                        _ = CurrentPaths.TryRemove(key, out _);
#nullable enable
                    }
                }
            }
        });
    }

    private async Task Client_OnError(object sender, VirtualClientErrorEventArgs args)
    {
        await Task.Run(() =>
        {
            if (sender is VirtualClient client)
            {
                if (ClientLookup.TryGetValue(client, out var key))
                {
                    if (Containers.TryGetValue(key, out var container))
                    {
#nullable disable
                        container.Client.OnStart -= Client_OnStart;
                        container.Client.OnError -= Client_OnError;
                        container.Client.OnShutdown -= Client_OnShutdown;
                        container.Client.Dispose();

                        container.Client = null;
                        container.Loaded = false;
                        container.Errored = true;
                        container.Shutdown = true;

                        container.MessageStack.Push(args.Message);

                        UpdateCache(key);

                        // Remove from current paths now as the system is already stopped.
                        _ = CurrentPaths.TryRemove(key, out _);
#nullable enable
                        Log.Warning("Client {key} errored with {message}", key, args.Message);
                    }
                }
            }
        });
    }
    #endregion
}
