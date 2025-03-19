using Hubs.Core;

namespace Hubs.Abstractions;

public interface IConnection
{
    Task Send(HubMessage hubMessage);
}