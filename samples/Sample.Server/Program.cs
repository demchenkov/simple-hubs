using Microsoft.Extensions.Hosting;
using Hubs.Abstractions;
using Hubs.WebSocketHub;
using Sample.Server;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddWebSocketHub<TestHub>(options =>
{
    options.ProtocolHandlerType = typeof(SimpleProtocolHandler);
});

var app = builder.Build();

await app.RunAsync();


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
public class Test
{
    public int A { get; set; }
    public string? B { get; set; }
    public char C { get; set; }
    public bool D { get; set; }
}

public record TestRec(int A, string B, int C, bool D);

public class TestHub : Hub
{
    public async Task Ping()
    {
        foreach (var client in Clients)
        {
            await client.SendAsync("pong", new {});
        }
    }
    
    public Task<int> GetInt() { return Task.FromResult(0); }
    public Task<long> GetLong() { return Task.FromResult(0L); }
    public Task<bool> GetBool() { return Task.FromResult(true); }
    public Task<object> GetObj() { return Task.FromResult(new object()); }
    public async Task<object> GetAnon() { return new { a = 1, b = "2", c = '3', d = true }; }
    public async Task<Test> GetClass() { return new Test() { A = 1, B = "2", C = '3', D = true }; }
    public async Task<TestRec> GetRecord() { return new TestRec(1, "2", '3', true ); }
    
    public async Task Pong()
    {
        foreach (var client in Clients)
        {
            int a = await client.InvokeAsync<int>("pong", new {});
        }
    }
}

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously