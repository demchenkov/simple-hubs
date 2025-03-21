using Hubs.Core;

namespace Hubs.Abstractions;

public interface IConnection
{
    Task SendAsync(HubMessage hubMessage, CancellationToken cancellationToken = default);
}