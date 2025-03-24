using System.Collections.Immutable;

namespace Hubs.Core.ClientGeneration;

public readonly record struct InterfaceGenerationInfo(string InterfaceName, ImmutableArray<string> MethodSignatures)
{
    public string GeneratedClassName { get; } = InterfaceName.Split(':').Last().Split('.').Last().TrimStart('I');
    
    public bool Equals(InterfaceGenerationInfo other)
    {
        return InterfaceName == other.InterfaceName &&
               MethodSignatures.SequenceEqual(other.MethodSignatures);
    }
    
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(InterfaceName);
        
        foreach (var method in MethodSignatures)
        {
            hash.Add(method);
        }
    
        return hash.ToHashCode();
    }
}