using Hubs.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Hubs.Core;

internal static class DependencyInjection
{
    public static IServiceCollection AddHubCore<THub>(
        this IServiceCollection services,
        Action<HubServerOptions<THub>> configureOptions) where THub : Hub
    {
        services.Configure(configureOptions);

        services.TryAddSingleton<ActiveRequestStore>();
        services.TryAddSingleton<HubMethodResolver<THub>>();
        services.TryAddScoped<ProtocolHandlerFactory>();
        
        services.TryAddScoped<HubCallerContext>();

        services.TryAddTransient<IPortResolver, PortResolver>();
        services.TryAddTransient<ClientMessageHandler<THub>>();

        return services;
    }
}

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
        var serverOptions = _serviceProvider.GetRequiredService<IOptions<HubServerOptions<THub>>>();
        var protocolHandlerType = serverOptions.Value?.ProtocolHandlerType ?? throw new NullReferenceException();
        var protocolHandler = (IHubProtocolHandler)ActivatorUtilities.CreateInstance(_serviceProvider, protocolHandlerType);
        
        return protocolHandler;
    }
}