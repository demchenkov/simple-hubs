using System.Collections.Concurrent;
using Hubs.Core;

namespace Hubs.Abstractions;

public class HubCallerContext
{
    public string? ConnectionId { get; internal set; }
    public HubRequest? Request { get; internal set; }
    public ConcurrentDictionary<object, object?> Items { get; } = new ();
}