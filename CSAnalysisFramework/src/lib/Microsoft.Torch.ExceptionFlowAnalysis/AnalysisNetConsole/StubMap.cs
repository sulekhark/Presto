using System.Collections.Generic;
using System.Text;
using System.Linq;
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
            Map.Add("System.Threading.Tasks.VoidTaskResult", "Microsoft.Torch.Stubs.VoidTaskResult");
            Map.Add("System.Runtime.CompilerServices.AsyncTaskMethodBuilder", "Microsoft.Torch.Stubs.AsyncTaskMethodBuilder");
            Map.Add("System.Runtime.CompilerServices.AsyncTaskMethodBuilder<TResult>", "Microsoft.Torch.Stubs.AsyncTaskMethodBuilder<TResult>");
            Map.Add("System.Threading.Tasks.Task", "Microsoft.Torch.Stubs.Task");
            Map.Add("System.Threading.Tasks.Task<TResult>", "Microsoft.Torch.Stubs.Task<TResult>");
            Map.Add("System.Runtime.CompilerServices.TaskAwaiter", "Microsoft.Torch.Stubs.TaskAwaiter");
            Map.Add("System.Runtime.CompilerServices.TaskAwaiter<TResult>", "Microsoft.Torch.Stubs.TaskAwaiter<TResult>");
            NameToTypeDefMap = new Dictionary<string, ITypeDefinition>();
        }

        public static void SetupStubs(IModule stubsModule)
        {
            foreach (ITypeDefinition ty in stubsModule.GetAllTypes().OfType<INamedTypeDefinition>().ToList())
            {
                NameToTypeDefMap.Add(ty.FullName(), ty);
            }
        }
    }
}