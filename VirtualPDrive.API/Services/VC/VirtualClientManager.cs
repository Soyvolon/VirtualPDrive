using Serilog;

using System.Collections.Concurrent;

using VirtualPDrive.API.Structures.VC;
using VirtualPDrive.Client;

namespace VirtualPDrive.API.Services.VC;

public class VirtualClientManager : IVirtualClientManager
{
    private readonly IConfiguration _configuration;

    private ConcurrentDictionary<string, VirtualClientContainer> Containers { get; init; } = new();
    private ConcurrentDictionary<VirtualClient, string> ClientLookup { get; init; } = new();
    private ConcurrentDictionary<string, Timer> ErrorCacheTimers { get; set; } = new();

    private int _cacheTime = 30;

    public VirtualClientManager(IConfiguration configuration)
    {
        _configuration = configuration;
        _cacheTime = _configuration.GetValue<int>("ClientErrorCacheTime", 30);
    }

    public VirtualClientContainer CreateVirtualClient(VirtualClientSettings settings)
    {
        var client = new VirtualClient(settings);
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

        _ = Task.Run(async () =>
        {
            try
            {
                await client.StartAsync();
            }
            catch (Exception ex)
            {
                await Client_OnError(client, new()
                {
                    Exception = ex,
                    Message = ex.Message
                });
            }
        });

        return container;
    }

    public VirtualClientContainer? DestroyVirtualClient(string id)
    {
        if (Containers.TryRemove(id, out var container))
        {
            if (container.Client is not null)
            {
                _ = ClientLookup.TryRemove(container.Client, out _);

                container.Client.Dispose();
            }

            _ = ErrorCacheTimers.TryRemove(id, out _);

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
    private async Task Client_OnStart(object sender, VirtualClientEventArgs args)
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
                        container.Client.Dispose();

                        container.Client = null;
                        container.Loaded = false;
                        container.Errored = false;
                        container.Shutdown = true;

                        container.MessageStack.Push("Client gracefully shutdown.");

                        UpdateCache(key);
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
                        container.Client.Dispose();

                        container.Client = null;
                        container.Loaded = false;
                        container.Errored = true;
                        container.Shutdown = true;

                        container.MessageStack.Push(args.Message);

                        UpdateCache(key);
#nullable enable
                        Log.Warning("Client {key} errored with {message}", key, args.Message);
                    }
                }
            }
        });
    }
    #endregion
}
