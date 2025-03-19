using Hubs.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Hubs.Core;

public class ProtocolHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ProtocolHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IHubProtocolHandler Create<THub>()
        where THub : Hub
    {
        var serverOptions = _serviceProvider.GetRequiredService<IOptionsSnapshot<HubServerOptions>>();
        
        var type = serverOptions.Get(typeof(THub).FullName)?.ProtocolHandlerType ?? throw new NullReferenceException();
        var instance = (IHubProtocolHandler)ActivatorUtilities.CreateInstance(_serviceProvider, type);
        
        return instance;
    }
}