using System.Collections;
using Fleck;
using Hubs.Abstractions;
using Hubs.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Hubs.WebSocketHub;

internal sealed class HubClientsProxy : IHubClients
{
    private readonly WebSocketClientStore _clients;
    private readonly Func<IHubProtocolHandler> _protocolHandlerFactoryFactory;
    private readonly IServiceProvider _serviceProvider;

    public HubClientsProxy(
        WebSocketClientStore clients,
        Func<IHubProtocolHandler> protocolHandlerFactory,
        IServiceProvider serviceProvider)
    {
        _clients = clients;
        _serviceProvider = serviceProvider;
        _protocolHandlerFactoryFactory = protocolHandlerFactory;
    }

    public IHubClient Client(string connectionId)
    {
        if (_clients.TryGetValue(connectionId, out var client))
        {
            return HubClient(client);
        }
        
        throw new KeyNotFoundException();
    }

    private IHubClient HubClient(IWebSocketConnection client)
    {
        var protocolHandler = _protocolHandlerFactoryFactory();
        var connection = new ConnectionAdapter(client, protocolHandler);
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