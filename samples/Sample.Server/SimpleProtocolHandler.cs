using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hubs.Abstractions;
using Hubs.Core;

namespace Sample.Server;

internal class SimpleProtocolHandler(HubCallerContext callerContext) : IHubProtocolHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
    private const string RawRequest = "RawRequest";

    public HubMessage DeserializeClientMessage(ReadOnlyMemory<byte> message)
    {
        var body = Encoding.UTF8.GetString(message.Span);
        var rawMessage = JsonSerializer.Deserialize<RawMessage>(body);

        // TODO: Verify signature
        
        if (rawMessage == null)
            throw new NullReferenceException();

        callerContext.Items[RawRequest] = rawMessage;

        var requestId = rawMessage.Arguments.TryGetValue("requestId", out var value) ? value.GetString() : null;
        
        if (string.IsNullOrEmpty(requestId))
            requestId = Guid.NewGuid().ToString();
        
        // client cannot send responses to server yet.
        return new HubRequest(rawMessage.Method, rawMessage.Arguments, requestId);
    }

    public ReadOnlyMemory<byte> SerializeServerMessage(HubMessage payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        if (payload is HubResponse serverResponse)
        {
            var request = callerContext.Request ?? throw new NullReferenceException();

            var rawRequest = (callerContext.Items.GetValueOrDefault(RawRequest, null) as string) ?? string.Empty;
            var response = serverResponse.Data is not null ? JsonSerializer.Serialize(serverResponse.Data) : string.Empty;
            
            var message = new RawMessage(request.Method, rawRequest, response);
            return SerializeInternal(message);
        }
        
        if (payload is HubRequest serverRequest)
        {
            // Pass server message into response field
            var response = serverRequest.Arguments.Count > 0 ? JsonSerializer.Serialize(serverRequest.Arguments) : string.Empty;
            
            var message = new RawMessage(serverRequest.Method, string.Empty, response);
            return SerializeInternal(message);
        }
        
        throw new NotSupportedException("Unsupported message type.");
    }

    private static ReadOnlyMemory<byte> SerializeInternal(RawMessage message)
    {
        var json = JsonSerializer.Serialize(message, JsonOptions);
        return new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));
    }


    private record RawMessage(
        [property: JsonPropertyName("method")] string Method,
        [property: JsonPropertyName("request")] string Request,
        [property: JsonPropertyName("response")] string Response,
        [property: JsonPropertyName("signature")] string? Signature = null
    )
    {
        private Dictionary<string, JsonElement>? _arguments;
        
        [JsonIgnore]
        public Dictionary<string, JsonElement> Arguments => 
            _arguments ??= string.IsNullOrEmpty(Request) 
                ? new Dictionary<string, JsonElement>() 
                : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Request)!;
    }
}