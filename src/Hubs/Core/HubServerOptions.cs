namespace Hubs.Core;

public class HubServerOptions
{
    public int MaxConcurrentRequests { get; set; } = 10;
    public int PreferredPort { get; set; } = 5000;
    public Type ProtocolHandlerType { get; set; } = null!;
}