using System.Net;
using System.Text;
using System.Threading.Channels;
using Fleck;
using Hubs.Abstractions;
using Hubs.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Hubs.WebSocketHub;

internal class WebSocketServer<TAssociatedHub> : IHostedService
    where TAssociatedHub : Hub
{
    private readonly Channel<MessageMetaInfo> _messageQueue = Channel.CreateUnbounded<MessageMetaInfo>();
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IPortResolver _portResolver;
    private readonly HubServerOptions _options;
    
    private WebSocketServer _server = null!;

    public WebSocketServer(
        IServiceScopeFactory serviceScopeFactory,
        IPortResolver portResolver,
        IOptionsSnapshot<HubServerOptions> options,
        WebSocketClientStore clients)
    {
        Clients = clients;
        _serviceScopeFactory = serviceScopeFactory;
        _portResolver = portResolver;
        _options = options.Get(typeof(TAssociatedHub).FullName);
    }
    
    internal WebSocketClientStore Clients { get; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var port = _portResolver.FindFreePort(_options.PreferredPort);
        var address = IPAddress.Loopback;
        
        _server = new WebSocketServer($"ws://{address}:{port}");
        
        _server.Start(socket =>
        {
            socket.OnOpen = () => SocketOnOpen(socket);
            socket.OnClose = () => SocketOnClose(socket);
            socket.OnError = ex => SocketOnClose(socket, ex);
            socket.OnMessage = body => SocketOnMessage(socket, body);
        });

        await RunWorkers(_options.MaxConcurrentRequests, cancellationToken);
    }

    private async Task RunWorkers(int workerCount, CancellationToken cancellationToken)
    {
        var tasks = Enumerable
            .Range(0, workerCount)
            .Select(_ => Task.Run(Worker, cancellationToken));
        
        await Task.WhenAll(tasks);
        
        return;

        async Task Worker()
        {
            await foreach (var message in _messageQueue.Reader.ReadAllAsync(cancellationToken))
            {
                using var scope = CreateHubScope(message.ConnectionId, message.Socket);
                var processor = scope.ServiceProvider.GetRequiredService<ClientMessageHandler<TAssociatedHub>>();
                var bytes = Encoding.UTF8.GetBytes(message.Body);
                await processor.HandleMessageAsync(bytes, cancellationToken);
            }
        }
    }

    private void SocketOnOpen(IWebSocketConnection socket)
    {
        if (!Clients.TryAdd(socket))
            return;

        var connectionId = socket.ConnectionInfo.Id.ToString();
        using var scope = CreateHubScope(connectionId, socket);
        var hub = scope.ServiceProvider.GetRequiredService<IHubFactory<TAssociatedHub>>().Create();
        hub.OnConnectedAsync().ConfigureAwait(false);
    }
    
    private void SocketOnClose(IWebSocketConnection socket, Exception? ex = null)
    {
        if (!Clients.TryRemove(socket)) 
            return;
        
        var disconnectedId = socket.ConnectionInfo.Id.ToString();
        using var scope = CreateHubScope(disconnectedId, socket);
        var hub = scope.ServiceProvider.GetRequiredService<IHubFactory<TAssociatedHub>>().Create();
        hub.OnDisconnectedAsync(ex).ConfigureAwait(false);
    }

    private void SocketOnMessage(IWebSocketConnection socket, string body)
    {
        var connectionId = socket.ConnectionInfo.Id.ToString();
        if (connectionId is null) throw new InvalidOperationException("Invalid connectionId");

        _messageQueue.Writer.TryWrite(new MessageMetaInfo(connectionId, socket, body));
    }

    private IServiceScope CreateHubScope(string connectionId, IWebSocketConnection _)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<HubCallerContext>();
        context.ConnectionId = connectionId;

        return scope;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _server.Dispose();
        _messageQueue.Writer.Complete();
        
        return Task.CompletedTask;
    }

    private record MessageMetaInfo(string ConnectionId, IWebSocketConnection Socket, string Body);
}