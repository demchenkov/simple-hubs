using System.Text.Json;

namespace Hubs.Core;

public abstract record HubMessage;

public record HubRequest(string Method, IReadOnlyDictionary<string, JsonElement> Arguments, string RequestId) : HubMessage;
public record HubResponse(string RequestId, object? Data) : HubMessage;