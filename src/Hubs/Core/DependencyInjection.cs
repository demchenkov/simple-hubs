using Hubs.Abstractions;
using Hubs.WebSocketHub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hubs.Core;

internal static class DependencyInjection
{
    public static IServiceCollection AddHubCore<THub>(
        this IServiceCollection services,
        Action<HubServerOptions> configureOptions) where THub : Hub
    {
        services.Configure(typeof(THub).FullName, configureOptions);

        services.TryAddSingleton<ActiveRequestStore>();
        services.TryAddSingleton<HubMethodResolver<THub>>();
        services.TryAddScoped<ProtocolHandlerFactory>();
        
        services.TryAddScoped<HubCallerContext>();

        services.TryAddTransient<ClientMessageHandler<THub>>();

        return services;
    }
}