//HintName: Contract1.g.cs
using System;

namespace Hubs.InterfaceGenerators
{
    public class Contract1Impl : IContract1
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
    }
}