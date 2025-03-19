namespace Hubs.WebSocketHub;

public interface IPortResolver
{
    public int FindFreePort(int preferredPort);
}