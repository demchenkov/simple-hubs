# Simple-Hubs

Simple-Hubs is a lightweight alternative of SignalR, designed to provide real-time communication with minimal overhead. It supports only Sockets and WebSockets, making it an ideal choice for projects that require efficient, scalable real-time messaging without unnecessary complexity.

## Features
- **Minimal Overhead**: Focuses on simplicity, avoiding unnecessary abstractions.
- **Sockets & WebSockets Only**: No support for long polling or server-sent events, ensuring a streamlined architecture.
- **Efficient Message Handling**: Provides a simple API for sending and receiving messages.
- **Scalable Design**: Optimized for handling multiple concurrent connections with minimal resource usage.
- **Easy Integration**: Designed to work seamlessly with .NET applications.

## Getting Started

### Basic Usage
#### Server Side
```csharp
var builder = Host.CreateApplicationBuilder();

builder.Services.AddWebSocketHub<TestHub>(options =>
{
    options.ProtocolHandlerType = typeof(SimpleProtocolHandler);
});

var app = builder.Build();

await app.RunAsync();
```

#### Client Side
```csharp
using SignalRLight;

var client = new SignalRLightClient("ws://localhost:5000");
client.Connect();

client.OnMessageReceived += (message) =>
{
    Console.WriteLine($"Server says: {message}");
};

client.SendMessage("Hello Server");
```

## Roadmap
- [ ] Native Socket support
- [ ] Authentication support
- [ ] Load balancing across multiple servers
- [ ] Built-in message serialization
- [ ] Improved reconnection strategies

## Contributing
Contributions are welcome! Feel free to submit issues and pull requests.

## License
MIT License

