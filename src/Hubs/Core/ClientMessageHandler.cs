using System.Reflection;
using Hubs.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hubs.Core;

internal sealed class ClientMessageHandler<TAssociatedHub>
    where TAssociatedHub : Hub
{
    private readonly ActiveRequestStore _activeRequestStore;
    private readonly IHubProtocolHandler _protocolHandler;
    private readonly ILogger<ClientMessageHandler<TAssociatedHub>> _logger;
    private readonly HubMethodResolver<TAssociatedHub> _hubMethodResolver;
    private readonly IServiceProvider _serviceProvider;

    public ClientMessageHandler(
        ProtocolHandlerFactory protocolHandlerFactory,
        ILogger<ClientMessageHandler<TAssociatedHub>> logger,
        HubMethodResolver<TAssociatedHub> hubMethodResolver,
        ActiveRequestStore activeRequestStore,
        IServiceProvider serviceProvider)
    {
        _protocolHandler = protocolHandlerFactory.Create<TAssociatedHub>();
        _logger = logger;
        _hubMethodResolver = hubMethodResolver;
        _activeRequestStore = activeRequestStore;
        _serviceProvider = serviceProvider;
    }

    public async Task HandleMessageAsync(ReadOnlyMemory<byte> body, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = _protocolHandler.DeserializeClientMessage(body);

            var result = message switch
            {
                HubRequest clientRequest => HandleClientRequest(clientRequest, cancellationToken),
                HubResponse clientResponse => HandleClientResponse(clientResponse),
                _ => throw new ArgumentOutOfRangeException(nameof(message), "Invalid base type.")
            };

            await result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while processing hub message");
        }
    }

    private async Task HandleClientRequest(HubRequest request, CancellationToken cancellationToken)
    {
        var (method, @params) = _hubMethodResolver.Resolve(request.Method, request.Arguments, cancellationToken);
        var hub = _serviceProvider.GetRequiredService<IHubFactory<TAssociatedHub>>().Create();
        hub.CallerContext.Request = request;
        
        var result = method.Invoke(hub, @params);

        if (result is not Task task) 
            throw new NotSupportedException("The hub method is not supported.");
        
        await task.ConfigureAwait(false);
            
        if (IsTaskWithoutResult(task, out var resultProperty))
            return;
        
        var response = resultProperty?.GetValue(task);
        var serverResponse = new HubResponse(request.RequestId, response);

        if (hub.Clients.Client(hub.CallerContext.ConnectionId!) is not HubClient hubClient)
            throw new InvalidOperationException("Could not send response to client.");
            
        await hubClient.SendAsync(serverResponse, cancellationToken);
    }

    private Task HandleClientResponse(HubResponse message)
    {
        if (_activeRequestStore.TryGetValue(message.RequestId, out var tcs))
        {
            tcs.TrySetResult(message);
        }
        
        return Task.CompletedTask;
    }
    
    private static bool IsTaskWithoutResult(Task task, out PropertyInfo? resultProperty)
    {
        resultProperty = task.GetType().GetProperty("Result");
        var taskType = task.GetType();
    
        if (resultProperty is null || !taskType.IsGenericType)
            return true;
        
        var genericArgs = taskType.GetGenericArguments();
        if (genericArgs.Length == 1 && genericArgs[0].FullName == "System.Threading.Tasks.VoidTaskResult")
            return true;
        
        if (resultProperty.PropertyType.FullName == "System.Threading.Tasks.VoidTaskResult")
            return true;
        
        return false;
    }
}