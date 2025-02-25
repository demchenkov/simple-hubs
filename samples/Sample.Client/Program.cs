using System.Net.WebSockets;
using System.Text;

await ConnectToWebSocket();
Console.ReadKey();
return;

static async Task ConnectToWebSocket()
{
    using var ws = new ClientWebSocket();
    var serverUri = new Uri("ws://127.0.0.1:5000");
            
    try
    {
        Console.WriteLine("Connecting to WebSocket server...");
        await ws.ConnectAsync(serverUri, CancellationToken.None);
        Console.WriteLine("Connected to WebSocket server");

        // Start receiving messages in background
        _ = ReceiveMessages(ws);

        // Send messages in a loop
        while (ws.State == WebSocketState.Open)
        {
            Console.Write("Enter message to send (or 'exit' to quit): ");
            var message = Console.ReadLine();
                    
            if (string.IsNullOrEmpty(message))
                continue;
                        
            if (message.ToLower() == "exit")
                break;

            var messageBytes = Encoding.UTF8.GetBytes(message);
            await ws.SendAsync(
                new ArraySegment<byte>(messageBytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        // Close the WebSocket if it's still open
        if (ws.State == WebSocketState.Open)
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                "Closing", CancellationToken.None);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

static async Task ReceiveMessages(ClientWebSocket ws)
{
    var buffer = new byte[1024];
        
    try
    {
        while (ws.State == WebSocketState.Open)
        {
            var result = await ws.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, 
                    "Acknowledge Close", CancellationToken.None);
                Console.WriteLine("WebSocket connection closed");
                break;
            }

            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"Received: {receivedMessage}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error receiving messages: {ex.Message}");
    }
}