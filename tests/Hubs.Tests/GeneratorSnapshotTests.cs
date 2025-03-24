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
        
        return GeneratorTestHelper.Verify(source);
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
        
        return GeneratorTestHelper.Verify(source);
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
        
        return GeneratorTestHelper.Verify(source);
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
        
        return GeneratorTestHelper.Verify(source);
    }
}