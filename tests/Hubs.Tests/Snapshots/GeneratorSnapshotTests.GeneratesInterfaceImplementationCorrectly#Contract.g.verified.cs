//HintName: Contract.g.cs
using System;

namespace Hubs.InterfaceGenerators
{
    public class ContractImpl : IContract
    {
        async System.Threading.Tasks.Task IContract.Method1() 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task IContract.Method2(int i) 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task IContract.Method3(object i) 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task<int> IContract.Method4() 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task<int> IContract.Method5(int i) 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task<int> IContract.Method6(object i) 
        {
            throw new NotImplementedException();
        }
    }
}