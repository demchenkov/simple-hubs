using Hubs.Core;

namespace Hubs.Abstractions;

public interface IConnection
{
    Task Send(HubMessage hubMessage);
}

public interface IHubFactory<out THub>
    where THub : Hub
{
    THub Create();
}