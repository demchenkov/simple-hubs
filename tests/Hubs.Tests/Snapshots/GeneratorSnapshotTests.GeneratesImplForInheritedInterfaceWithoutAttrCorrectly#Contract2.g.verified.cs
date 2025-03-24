//HintName: Contract2.g.cs
using System;

namespace Hubs.InterfaceGenerators
{
    public class Contract2Impl : IContract2
    {
        async System.Threading.Tasks.Task IContract1.Method1() 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task IContract1.Method2(int i) 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task IContract1.Method3(object i) 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task<int> IContract2.Method4() 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task<int> IContract2.Method5(int i) 
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task<int> IContract2.Method6(object i) 
        {
            throw new NotImplementedException();
        }
    }
}