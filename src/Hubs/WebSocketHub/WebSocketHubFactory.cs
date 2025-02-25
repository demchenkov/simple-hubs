using Hubs.Abstractions;
using Hubs.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Hubs.WebSocketHub;

internal sealed class WebSocketHubFactory<THub> : IHubFactory<THub>
    where THub : Hub
{
    private readonly IServiceProvider _sp;

    public WebSocketHubFactory(IServiceProvider serviceProvider)
    {
        _sp = serviceProvider;
    }

    public THub Create()
    {
        var server = _sp.GetRequiredService<WebSocketServer<THub>>();
        var proxy = new HubClientsProxy(
            server.Clients,
            () => _sp.GetRequiredService<ProtocolHandlerFactory>().Create<THub>(), 
            _sp);
            
        var hub = ActivatorUtilities.CreateInstance<THub>(_sp);
        hub.Clients = proxy;
        hub.CallerContext = _sp.GetRequiredService<HubCallerContext>();
            
        return hub;
    }
}