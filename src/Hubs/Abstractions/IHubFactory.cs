namespace Hubs.Abstractions;

public interface IHubFactory<out THub>
    where THub : Hub
{
    THub Create();
}