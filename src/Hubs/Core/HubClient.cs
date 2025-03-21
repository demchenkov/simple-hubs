using System.Text.Json;
using Hubs.Abstractions;
using Microsoft.Extensions.Logging;

namespace Hubs.Core;

internal sealed class HubClient : IHubClient
{
    private readonly IConnection _connection;
    private readonly ActiveRequestStore _activeRequestStore;
    private readonly ILogger<HubClient> _logger;

    public HubClient(IConnection connection, ActiveRequestStore activeRequestStore, ILogger<HubClient> logger)
    {
        _connection = connection;
        _activeRequestStore = activeRequestStore;
        _logger = logger;
    }

    public async Task SendAsync(string method, object args, CancellationToken cancellationToken = default)
    {
        var request = new HubRequest(method, ConvertArgs(args), Guid.NewGuid().ToString());
        await SendAsync(request, cancellationToken);
    }

    public async Task<T> InvokeAsync<T>(string method, object args, CancellationToken cancellationToken = default)
    {
        var request = new HubRequest(method, ConvertArgs(args), Guid.NewGuid().ToString());
        return await SendAsync<T>(request, cancellationToken);
    }

    private static Dictionary<string, JsonElement> ConvertArgs(object args)
    {
        var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(args, options);
        return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;
    }
    
    internal async Task SendAsync(HubMessage hubMessage, CancellationToken cancellationToken = default)
    {
        try
        {
           await _connection.SendAsync(hubMessage, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while sending hub message");
        }
    }

    private async Task<T> SendAsync<T>(HubMessage hubMessage, CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        // TODO: extract to options? 
        cts.CancelAfter(TimeSpan.FromSeconds(30));
        try
        {
            var tcs = new TaskCompletionSource<HubResponse>(cts.Token);
        
            _activeRequestStore.Add(requestId, tcs);
            await _connection.SendAsync(hubMessage, cts.Token);
        
            var response = await tcs.Task;
            var json = JsonSerializer.Serialize(response.Data);
        
            return JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured while sending hub message");
            throw;
        }
        finally
        {
            _activeRequestStore.Remove(requestId);
        }
    }
}