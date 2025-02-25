using Hubs.Core;

namespace Hubs.Abstractions;

public interface IHubProtocolHandler
{
    public HubMessage DeserializeClientMessage(ReadOnlyMemory<byte> message);
    public ReadOnlyMemory<byte> SerializeServerMessage(HubMessage payload);
}