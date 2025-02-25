using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Fleck;

namespace Hubs.WebSocketHub;

public class WebSocketClientStore : IEnumerable<IWebSocketConnection>
{
    private readonly ConcurrentDictionary<string, IWebSocketConnection> _clients = new();
    
    public bool TryAdd(IWebSocketConnection socket)
    {
        return _clients.TryAdd(socket.ConnectionInfo.Id.ToString(), socket);
    }

    public bool TryRemove(IWebSocketConnection socket)
    {
        return _clients.TryRemove(socket.ConnectionInfo.Id.ToString(), out _);
    }

    public bool TryGetValue(string connectionId, out IWebSocketConnection o)
    {
        return _clients.TryGetValue(connectionId, out o);
    }

    public IEnumerator<IWebSocketConnection> GetEnumerator() => _clients.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}