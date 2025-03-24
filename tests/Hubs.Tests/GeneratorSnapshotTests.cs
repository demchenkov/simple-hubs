using Hubs.Abstractions.Attributes;
using Hubs.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Hubs.Tests;

public class GeneratorSnapshotTests
{
    [Fact]
    public Task GeneratesInterfaceImplementationCorrectly()
    {
        // The source code to test
        var source =
            """
            using System;
            using System.Threading.Tasks;
            using Hubs.Abstractions.Attributes;

            [HubClientContract]
            public interface IContract
            {
                Task Method1();
                Task Method2(int i);
                Task Method3(object i);
                
                Task<int> Method4();
                Task<int> Method5(int i);
                Task<int> Method6(object i);
            };
            """;
        
        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesImplForInheritedInterfaceWithoutAttrCorrectly()
    {
        // The source code to test
        var source =
            """
            using System;
            using System.Threading.Tasks;
            using Hubs.Abstractions.Attributes;

            [HubClientContract]
            public interface IContract1
            {
                Task Method1();
                Task Method2(int i);
                Task Method3(object i);
            }
            public interface IContract2 : IContract1 
            {
                Task<int> Method4();
                Task<int> Method5(int i);
                Task<int> Method6(object i);
            }
            """;
        
        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesImplForInheritedInterfaceWithAttrCorrectly()
    {
        // The source code to test
        var source =
            """
            using System;
            using System.Threading.Tasks;
            using Hubs.Abstractions.Attributes;

            [HubClientContract]
            public interface IContract1
            {
                Task Method1();
                Task Method2(int i);
                Task Method3(object i);
            }
            [HubClientContract]
            public interface IContract2 : IContract1 
            {
                Task<int> Method4();
                Task<int> Method5(int i);
                Task<int> Method6(object i);
            }
            """;
        
        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesImplForInterfaceInsideNamespaceCorrectly()
    {
        // The source code to test
        var source =
            """
            using System;
            using System.Threading.Tasks;
            using Hubs.Abstractions.Attributes;

            namespace TestNamespace 
            {
                [HubClientContract]
                public interface IContract
                {
                    Task Method1();
                    Task Method2(int i);
                    Task Method3(object i);
                    
                    
                    Task<int> Method4();
                    Task<int> Method5(int i);
                    Task<int> Method6(object i);
                }
            }
            """;
        
        return TestHelper.Verify(source);
    }
}

public static class TestHelper
{
    public static Task Verify(string source)
    {
        // Parse the provided string into a C# syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        IEnumerable<PortableExecutableReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(HubClientContractAttribute).Assembly.Location)
        ];
        
        // Create a Roslyn compilation for the syntax tree.
        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [syntaxTree],
            references: references);


        // Create an instance of our ClientInterfaceGenerator incremental source generator
        var generator = new ClientInterfaceGenerator();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        // Use verify to snapshot test the source generator output!
        return Verifier
            .Verify(driver)
            .UseDirectory("Snapshots")
            .DisableDiff();
    }
}