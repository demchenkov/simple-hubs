namespace Hubs.Core.ClientGeneration;

public static class SourceGenerationHelper
{
    public static string GenerateInterfaceImplementation(InterfaceGenerationInfo value)
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

        static IEnumerable<string> MethodsImplementation(InterfaceGenerationInfo value)
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