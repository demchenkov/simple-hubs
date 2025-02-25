namespace Hubs.Abstractions;

public interface IPortResolver
{
    public int FindFreePort(int preferredPort);
}