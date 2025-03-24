//HintName: Contract.g.cs
using System;

namespace Hubs.InterfaceGenerators
{
    public class ContractImpl : TestNamespace.IContract
    {
        async System.Threading.Tasks.Task TestNamespace.IContract.Method1() 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task TestNamespace.IContract.Method2(int i) 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task TestNamespace.IContract.Method3(object i) 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task<int> TestNamespace.IContract.Method4() 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task<int> TestNamespace.IContract.Method5(int i) 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task<int> TestNamespace.IContract.Method6(object i) 
        {
            throw new NotImplementedException();
        }
    }
}