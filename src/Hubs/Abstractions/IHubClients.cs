namespace Hubs.Abstractions;

public interface IHubClients : IEnumerable<IHubClient>
{
    IHubClient Client(string connectionId);
}

public interface IHubClient
{
    Task SendAsync(string method, object args, CancellationToken cancellationToken = default);
    Task<T> InvokeAsync<T>(string method, object args, CancellationToken cancellationToken = default);
}
