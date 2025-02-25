using System.Net;
using System.Net.Sockets;
using Hubs.Abstractions;

namespace Hubs.Core;

internal sealed class PortResolver : IPortResolver
{
    public int FindFreePort(int preferredPort)
    {
        var listener = new TcpListener(IPAddress.Loopback, preferredPort);
    
        try
        {
            listener.Start();
        
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            return port;
        }
        catch (SocketException)
        {
            return FindFreePort(0); // 0 will choose a free port
        }
        finally
        {
            // Always stop the listener
            listener.Stop();
        }
    }
}