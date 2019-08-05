using System.Collections.Generic;
using Microsoft.Cci;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole
{
    public static class StubMap
    {
        public static readonly IDictionary<string, string> Map;
        public static readonly IDictionary<string, ITypeDefinition> NameToTypeDefMap;


        static StubMap()
        {
            Map = new Dictionary<string, string>();
            Map.Add("System.Runtime.CompilerServices.AsyncTaskMethodBuilder", "Microsoft.Torch.Stubs.AsyncTaskMethodBuilder");
            NameToTypeDefMap = new Dictionary<string, ITypeDefinition>();
        }

        public static void SetupStubs(IModule stubsModule)
        {
            foreach(ITypeDefinition ty in stubsModule.GetAllTypes())
            {
                NameToTypeDefMap.Add(ty.ToString(), ty);
            }
        }
    }
}