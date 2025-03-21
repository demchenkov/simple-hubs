using Fleck;
using Hubs.Abstractions;
using Hubs.Core;

namespace Hubs.WebSocketHub;

internal sealed class ConnectionAdapter : IConnection
{
    private readonly IWebSocketConnection _socket;
    private readonly IHubProtocolHandler _protocolHandler;
    public ConnectionAdapter(IWebSocketConnection socket, IHubProtocolHandler protocolHandler)
    {
        _socket = socket;
        _protocolHandler = protocolHandler;
    }
        
    public Task SendAsync(HubMessage hubMessage, CancellationToken cancellationToken = default)
    {
        var bytes = _protocolHandler.SerializeServerMessage(hubMessage);
        return _socket.Send(bytes.ToArray());
    }
}