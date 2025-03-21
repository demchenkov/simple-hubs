namespace Hubs.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
public sealed class HubClientContractAttribute : Attribute
{
}