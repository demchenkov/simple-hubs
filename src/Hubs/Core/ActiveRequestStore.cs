using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Hubs.Core;

internal sealed class ActiveRequestStore
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<HubResponse>> _requests = new();

    public bool TryGetValue(string requestId, out TaskCompletionSource<HubResponse> o)
    {
        return _requests.TryGetValue(requestId, out o);
    }
    
    public void Add(string requestId, TaskCompletionSource<HubResponse> tcs)
    {
        _requests[requestId] = tcs;
    }

    public void Remove(string requestId)
    {
        _requests.TryRemove(requestId, out _);
    }
}