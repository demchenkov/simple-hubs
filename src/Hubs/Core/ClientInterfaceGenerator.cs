using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hubs.Core;

[Generator]
public class ClientInterfaceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // We're looking for methods with an attribute that are in an interface
        var candidateMethodsProvider = context.SyntaxProvider.CreateSyntaxProvider(
            static (syntax, cancellationToken) =>
                syntax is MethodDeclarationSyntax
                    {
                        Parent: InterfaceDeclarationSyntax,
                        AttributeLists.Count: > 0
                    },
            static (context, cancellationToken) => (MethodDeclarationSyntax)context.Node
        );

        // We also look for interfaces that derive from others, so we can see if any base methods contain
        // Refit methods
        var candidateInterfacesProvider = context.SyntaxProvider.CreateSyntaxProvider(
            (syntax, cancellationToken) =>
                syntax is InterfaceDeclarationSyntax { BaseList: not null },
            static (context, cancellationToken) => (InterfaceDeclarationSyntax)context.Node
        );

        var refitInternalNamespace = context.AnalyzerConfigOptionsProvider.Select(
            (analyzerConfigOptionsProvider, cancellationToken) =>
                analyzerConfigOptionsProvider.GlobalOptions.TryGetValue(
                    "build_property.RefitInternalNamespace",
                    out var refitInternalNamespace
                )
                    ? refitInternalNamespace
                    : null
        );

        var inputs = candidateMethodsProvider
            .Collect()
            .Combine(candidateInterfacesProvider.Collect())
            .Select(
                (combined, cancellationToken) =>
                    (candidateMethods: combined.Left, candidateInterfaces: combined.Right)
            )
            .Combine(refitInternalNamespace)
            .Combine(context.CompilationProvider)
            .Select(
                (combined, cancellationToken) =>
                    (
                        combined.Left.Left.candidateMethods,
                        combined.Left.Left.candidateInterfaces,
                        refitInternalNamespace: combined.Left.Right,
                        compilation: combined.Right
                    )
            );

            // var parseStep = inputs.Select(
            //     (collectedValues, cancellationToken) =>
            //     {
            //         return Parser.GenerateInterfaceStubs(
            //             (CSharpCompilation)collectedValues.compilation,
            //             collectedValues.refitInternalNamespace,
            //             collectedValues.candidateMethods,
            //             collectedValues.candidateInterfaces,
            //             cancellationToken
            //         );
            //     }
            // );
            //
            // // output the diagnostics
            // // use `ImmutableEquatableArray` to cache cases where there are no diagnostics
            // // otherwise the subsequent steps will always rerun.
            // var diagnostics = parseStep
            //     .Select(static (x, _) => x.diagnostics.ToImmutableEquatableArray())
            //     .WithTrackingName(RefitGeneratorStepName.ReportDiagnostics);
            // context.ReportDiagnostics(diagnostics);
            //
            // var contextModel = parseStep.Select(static (x, _) => x.Item2);
            // var interfaceModels = contextModel
            //     .SelectMany(static (x, _) => x.Interfaces)
            //     .WithTrackingName(RefitGeneratorStepName.BuildRefit);
            // context.EmitSource(interfaceModels);
            //
            // context.RegisterImplementationSourceOutput(
            //     contextModel,
            //     static (spc, model) =>
            //     {
            //         Emitter.EmitSharedCode(model, (name, code) => spc.AddSource(name, code));
            //     }
            // );
    }
}