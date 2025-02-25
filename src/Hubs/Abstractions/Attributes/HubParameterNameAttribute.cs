using System.Reflection;

namespace Hubs.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public class HubParameterNameAttribute : Attribute
{
    public string Name { get; }
 
    public HubParameterNameAttribute(string name)
    {
        Name = name;
    }
    
    public static string GetName(ParameterInfo parameterInfo)
    {
        if (parameterInfo == null)
            throw new ArgumentNullException(nameof(parameterInfo));
        
        var name = parameterInfo.GetCustomAttribute<HubParameterNameAttribute>()?.Name ?? parameterInfo.Name;
        return name ?? throw new InvalidOperationException("The parameter name is missing.");
    }
}