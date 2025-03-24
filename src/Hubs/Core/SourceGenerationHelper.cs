namespace Hubs.Core;

public static class SourceGenerationHelper
{
    public static string GenerateInterfaceImplementation(Example value)
    {
        var r = 
            $$"""
              using System;

              namespace Hubs.InterfaceGenerators
              {
                  public class {{value.GeneratedClassName}}Impl : {{value.InterfaceName}}
                  {
              {{string.Join("\n\n", MethodsImplementation(value))}}
                  }
              }
              """;

        return r;

        static IEnumerable<string> MethodsImplementation(Example value)
        {
            foreach (var signature in value.MethodSignatures)
            {
                yield return
                    $$"""
                              async {{signature}} 
                              {
                                  throw new NotImplementedException();
                              }
                      """;
            }
        }
    }
}