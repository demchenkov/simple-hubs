using Hubs.Abstractions.Attributes;

namespace Sample.Server;

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