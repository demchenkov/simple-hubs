using System.Collections;
using Fleck;
using Hubs.Abstractions;
using Hubs.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Hubs.WebSocketHub;

internal sealed class HubClientsProxy : IHubClients
{
    private readonly WebSocketClientStore _clients;
    private readonly IHubProtocolHandler _protocolHandler;
    private readonly IServiceProvider _serviceProvider;

    public HubClientsProxy(
        WebSocketClientStore clients,
        IHubProtocolHandler protocolHandler,
        IServiceProvider serviceProvider)
    {
        _clients = clients;
        _serviceProvider = serviceProvider;
        _protocolHandler = protocolHandler;
    }

    public IHubClient Client(string connectionId)
    {
        if (_clients.TryGetValue(connectionId, out var client))
        {
            return HubClient(client!);
        }
        
        throw new KeyNotFoundException();
    }

    private IHubClient HubClient(IWebSocketConnection client)
    {
        var connection = new ConnectionAdapter(client, _protocolHandler);
        return ActivatorUtilities.CreateInstance<HubClient>(_serviceProvider, connection);
    }

    private IEnumerable<IHubClient> Clients()
    {
        foreach (var client in _clients)
        {
            yield return HubClient(client);
        }
    }

    public IEnumerator<IHubClient> GetEnumerator() => Clients().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}