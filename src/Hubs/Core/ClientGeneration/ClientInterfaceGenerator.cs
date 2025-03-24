using System.Collections.Immutable;
using System.Text;
using Hubs.Abstractions.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator; try to avoid LINQ to boost rerformance

namespace Hubs.Core.ClientGeneration;

[Generator]
public class  ClientInterfaceGenerator : IIncrementalGenerator
{
    private static readonly string _clientInterfaceName = typeof(HubClientContractAttribute).FullName!;
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // We're looking for interfaces with an attribute
        var candidateInterfacesProvider = context
            .SyntaxProvider
            .ForAttributeWithMetadataName(
                _clientInterfaceName,
                predicate: static (syntax, _) => syntax is InterfaceDeclarationSyntax,
                transform: static (ctx, _) => GetInterfaceToGenerate(ctx.SemanticModel, (InterfaceDeclarationSyntax)ctx.TargetNode))
            .Where(x => x is not null);

        // We also look for interfaces that derive from others, so we can see if any base methods contain
        var candidateWithBaseInterfacesProvider = context
            .SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntax, _) => syntax is InterfaceDeclarationSyntax { BaseList: not null },
                transform: static (ctx, _) => GetInterfaceToGenerate(ctx.SemanticModel, (InterfaceDeclarationSyntax)ctx.Node))
            .Where(x => x is not null);

        var res = candidateInterfacesProvider.Collect()
            .Combine(candidateWithBaseInterfacesProvider.Collect())
            .SelectMany((tuple, _) =>
            {
                var combined = tuple.Left.AddRange(tuple.Right);
                var combinedWithoutDuplicates = new HashSet<InterfaceGenerationInfo?>(combined);
                return combinedWithoutDuplicates.ToImmutableArray();
            })
            .WithTrackingName("ClientInterfaceGenerator");
        
        context.RegisterImplementationSourceOutput(res,
            static (spc, source) => Execute(source, spc));
    }

    static void Execute(InterfaceGenerationInfo? source, SourceProductionContext context)
    {
        if (source is null)
            return;
        
        var value = source.Value;
        // generate the source code and add it to the output
        var result = SourceGenerationHelper.GenerateInterfaceImplementation(value);

        context.AddSource($"{value.GeneratedClassName}.g.cs", SourceText.From(result, Encoding.UTF8));
    }

    static InterfaceGenerationInfo? GetInterfaceToGenerate(SemanticModel semanticModel, InterfaceDeclarationSyntax interfaceDeclarationSyntax)
    {
        // Get the semantic representation of the enum syntax
        if (semanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax) is not INamedTypeSymbol interfaceSymbol)
        {
            // something went wrong
            return null;
        }

        if (!HasAttributeAsBaseInterface(interfaceSymbol))
            return null;
        
        var interfaceMembers = ImmutableArray<ISymbol>.Empty;
        
        foreach (var baseInterface in interfaceSymbol.AllInterfaces)
        {
            interfaceMembers = interfaceMembers.AddRange(baseInterface.GetMembers());
        }

        interfaceMembers = interfaceMembers.AddRange(interfaceSymbol.GetMembers());
        
        var methodSignatures = new List<string>(interfaceMembers.Length);
        
        var sb = new StringBuilder();
        foreach (var member in interfaceMembers)
        {
            if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary } methodSymbol)
            {
                if (methodSymbol.ReturnType.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks")
                {
                    sb.Append(methodSymbol.ReturnType.ToDisplayString());
                    sb.Append(' ');
                    sb.Append(methodSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : methodSymbol.ContainingNamespace.ToDisplayString() + '.');
                    sb.Append(methodSymbol.ContainingType.Name);
                    sb.Append('.');
                    sb.Append(methodSymbol.Name);
                    sb.Append('(');
                    sb.Append(string.Join(", ", methodSymbol.Parameters.Select(p => p.ToDisplayString())));
                    sb.Append(')');

                    methodSignatures.Add(sb.ToString());
                    
                    sb.Clear();
                }
            }
            else
            {
                // member.DeclaringSyntaxReferences[0].Span
                // TODO: add diagnostic message
                // interface should have only async methods as a contract
            }
        }
        
        return new InterfaceGenerationInfo(interfaceSymbol.ToDisplayString(), [..methodSignatures]);
    }

    private static bool HasAttributeAsBaseInterface(INamedTypeSymbol interfaceSymbol)
    {
        if (HasAttribute(interfaceSymbol))
            return true;
        
        foreach (var baseInterface in interfaceSymbol.AllInterfaces)
        {
            if (HasAttribute(baseInterface)) return true;
        }
        
        return false;

        bool HasAttribute(INamedTypeSymbol baseInterface)
        {
            foreach (var attributeData in baseInterface.GetAttributes())
            {
                if (attributeData.AttributeClass?.ToDisplayString() == _clientInterfaceName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}