using System.Reflection;

namespace Hubs.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class HubMethodNameAttribute : Attribute
{
    public string Name { get; }
 
    public HubMethodNameAttribute(string name)
    {
        Name = name;
    }

    public static string GetName(MethodInfo methodInfo)
    {
        if (methodInfo == null)
            throw new ArgumentNullException(nameof(methodInfo));

        var name = methodInfo.GetCustomAttribute<HubMethodNameAttribute>()?.Name ?? methodInfo.Name;
        return name ?? throw new InvalidOperationException("The method name is missing.");
    }
}