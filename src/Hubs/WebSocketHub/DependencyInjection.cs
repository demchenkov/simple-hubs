using Hubs.Abstractions;
using Hubs.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hubs.WebSocketHub;

public static class DependencyInjection
{
    public static IServiceCollection AddWebSocketHub<THub>(
        this IServiceCollection services,
        Action<HubServerOptions<THub>> configureOptions) where THub : Hub
    {
        services.AddHubCore(configureOptions);
        services.TryAddTransient<IHubFactory<THub>, WebSocketHubFactory<THub>>();
        
        services.TryAddSingleton<WebSocketServer<THub>>(sp =>
        {
            var store = new WebSocketClientStore(); // each server should have individual store
            var server = ActivatorUtilities.CreateInstance<WebSocketServer<THub>>(sp, store);

            return server;
        });
        services.AddHostedService(sp => sp.GetRequiredService<WebSocketServer<THub>>());
        
        return services;
    }
}