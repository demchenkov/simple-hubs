using System.Collections.Immutable;
using System.Text;
using Hubs.Abstractions.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Hubs.Core;

[Generator]
public class  ClientInterfaceGenerator : IIncrementalGenerator
{
    private static readonly string _clientInterfaceName = typeof(HubClientContractAttribute).FullName!;
    private static readonly string _clientMethodName = typeof(HubMethodNameAttribute).FullName!;
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // We're looking for methods with an attribute that are in an interface
        // TODO: uncomment when ready
        // var candidateMethodsProvider = context
        //     .SyntaxProvider
        //     .ForAttributeWithMetadataName(
        //         _clientMethodName,
        //         predicate: static (syntax, _) => syntax is MethodDeclarationSyntax { Parent: InterfaceDeclarationSyntax },
        //         transform: static (ctx, _) => GetInterfaceToGenerate(ctx.SemanticModel, (InterfaceDeclarationSyntax)ctx.TargetNode.Parent!))
        //     .Where(x => x is not null);

        // We also look for interfaces that derive from others, so we can see if any base methods contain
        var candidateInterfacesProvider = context
            .SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntax, _) => syntax is InterfaceDeclarationSyntax { BaseList: not null },
                transform: static (ctx, _) => GetInterfaceToGenerate(ctx.SemanticModel, (InterfaceDeclarationSyntax)ctx.Node))
            .Where(x => x is not null);

        context.RegisterSourceOutput(candidateInterfacesProvider,
            static (spc, source) => Execute(source, spc));
    }

    static void Execute(Example? source, SourceProductionContext context)
    {
        if (source is null)
            return;
        
        var value = source.Value;
        // generate the source code and add it to the output
        var result = SourceGenerationHelper.GenerateInterfaceImplementation(value);

        context.AddSource($"{value.InterfaceName.TrimStart('I')}.g.cs", SourceText.From(result, Encoding.UTF8));
    }

    static Example? GetInterfaceToGenerate(SemanticModel semanticModel, InterfaceDeclarationSyntax interfaceDeclarationSyntax)
    {
        // Get the semantic representation of the enum syntax
        if (semanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax) is not INamedTypeSymbol interfaceSymbol)
        {
            // something went wrong
            return null;
        }

        if (!HasAttributeAsBaseInterface(interfaceSymbol))
            return null;
        
        var interfaceMembers = ImmutableArray<ISymbol>.Empty; ;
        
        foreach (var baseInterface in interfaceSymbol.AllInterfaces)
        {
            interfaceMembers = interfaceMembers.AddRange(baseInterface.GetMembers());
        }

        interfaceMembers = interfaceMembers.AddRange(interfaceSymbol.GetMembers());
        
        var members = new List<IMethodSymbol>(interfaceMembers.Length);
        
        foreach (var member in interfaceMembers)
        {
            if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary } methodSymbol)
            {
                if (methodSymbol.ReturnType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks")
                    members.Add(methodSymbol);
            }
            else
            {
                // TODO: add diagnostic message
                // interface should have only async methods as a contract
            }
        }

        var interfaceName = interfaceSymbol.Name;
        return new Example(interfaceName, members);
    }

    private static bool HasAttributeAsBaseInterface(INamedTypeSymbol interfaceSymbol)
    {
        foreach (var baseInterface in interfaceSymbol.AllInterfaces)
        {
            foreach (var attributeData in baseInterface.GetAttributes())
            {
                if (attributeData.AttributeClass?.ToDisplayString() == _clientInterfaceName)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
}

public static class SourceGenerationHelper
{
    public static string GenerateInterfaceImplementation(Example value)
    {
        var r = 
$$"""
using System;
using System.Threading.Tasks;

namespace Hubs.InterfaceGenerators
{
    public class {{value.InterfaceName}}Impl : {{value.InterfaceName}}
    {
{{string.Join("\n\n", MethodsImplementation(value))}}
    }
}
""";

        return r;

        static IEnumerable<string> MethodsImplementation(Example value)
        {
            foreach (var method in value.Methods)
            {
                yield return
$$"""
        public async {{method.ReturnType.Name}} {{method.Name}}({{string.Join(", ", method.Parameters.Select(x => $"{x.Type.ToDisplayString()} {x.Name}"))}}) 
        {
            throw new NotImplementedException();
        }
""";
            }
        }
    }
}

public record struct Example(string InterfaceName, List<IMethodSymbol> Methods);