namespace Hubs.Abstractions;

public abstract class Hub
{
    public IHubClients Clients { get; set; } = null!;
    public HubCallerContext CallerContext { get; set; } = null!;
 
    public virtual Task OnConnectedAsync()
    {
        return Task.CompletedTask;
    }
 
    public virtual Task OnDisconnectedAsync(Exception? exception)
    {
        return Task.CompletedTask;
    }
}
