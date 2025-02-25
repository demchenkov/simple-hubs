using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Hubs.Abstractions.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Hubs.Core;

internal class HubMethodResolver<TAssociatedHub>
{
    private readonly ConcurrentDictionary<string, MethodInfo> _methodCache = new ();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILookup<string, MethodInfo> _methods;

    public HubMethodResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        var type = typeof(TAssociatedHub);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        
        if (methods.Any(x => !IsTaskType(x.ReturnType)))
            throw new ArgumentOutOfRangeException(type.FullName, "All Hub's public methods must return Task or Task<T>.");
        
        _methods = methods.ToLookup(HubMethodNameAttribute.GetName, StringComparer.InvariantCultureIgnoreCase);
    }

    public (MethodInfo method, object?[] parameters) Resolve(
        string method,
        IReadOnlyDictionary<string, JsonElement> arguments,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{method}_{string.Join(":", arguments.Keys.Select(x => x.ToLower()))}";

        if (_methodCache.TryGetValue(cacheKey, out var result))
        {
            return MapArgsToParamsOrDefault(result, arguments, cancellationToken)!;
        }
        
        var methods = _methods[method]
            .Select(x => MapArgsToParamsOrDefault(x, arguments, cancellationToken))
            .Where(x => x.parameters is not null)
            .Cast<(MethodInfo method, object?[] parameters)>()
            .ToArray();
        
        if (!methods.Any())
        {
            throw new InvalidOperationException($"Method '{method}' not found.");
        }

        if (methods.Length > 1)
        {
            throw new InvalidOperationException($"Found more than one suitable '{method}' method.");
        }

        var pair = methods.First();
        _methodCache[cacheKey] = pair.method;
        
        return pair;
    }
    
    private (MethodInfo method, object?[]? parameters) MapArgsToParamsOrDefault(
        MethodInfo method,
        IReadOnlyDictionary<string, JsonElement> arguments,
        CancellationToken cancellationToken)
    {
        var parameters = method.GetParameters();
        return (method, DeserializeArguments(arguments, parameters, cancellationToken));
    }

    private object?[]? DeserializeArguments(
        IReadOnlyDictionary<string, JsonElement> arguments,
        ParameterInfo[] parameters,
        CancellationToken cancellationToken)
    {
        try
        {
            // create new dictionary to make argument lookup case-insensitive 
            var dict = arguments.ToDictionary(
                x => x.Key,
                x => x.Value, StringComparer.OrdinalIgnoreCase);

            return parameters
                .Select(param =>
                {
                    if (param.ParameterType == typeof(CancellationToken))
                    {
                        return cancellationToken;
                    }
                    
                    var service = _serviceProvider.GetRequiredService<IServiceProviderIsService>();
                    if (service.IsService(param.ParameterType))
                    {
                        // injection Service into method 
                        return _serviceProvider.GetService(param.ParameterType);
                    }
                    
                    var paramName = HubParameterNameAttribute.GetName(param);
                    if (dict.TryGetValue(paramName, out var jsonValue))
                    {
                        return jsonValue.Deserialize(param.ParameterType);
                    }

                    if (param.HasDefaultValue)
                    {
                        return param.DefaultValue;
                    }
                    
                    throw new Exception($"Missing argument: {param.Name}");
                })
                .ToArray();
        }
        catch
        {
            return null;
        }
    }
    
    private static bool IsTaskType(Type returnType)
    {
        return returnType == typeof(Task) || 
               (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>));
    }
}