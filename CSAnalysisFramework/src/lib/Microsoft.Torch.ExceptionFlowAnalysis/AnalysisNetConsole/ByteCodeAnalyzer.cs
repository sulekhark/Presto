using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Analyses;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Model;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole
{
    public static class ByteCodeAnalyzer
    {
        static ISet<ITypeDefinition> classes;
        static ISet<ITypeDefinition> types;
        static ISet<IMethodDefinition> methods;
        static ISet<IMethodDefinition> entryPtMethods;
        static ISet<IModule> modules;

        public static void GenerateEDBFacts(string input)
        {
            using (var host = new PeReader.DefaultHost())
            {
                Types.Initialize(host);
                var visitor = new MetadataVisitor(host, null);
                var assembly = new Assembly(host);
                assembly.Load(input);
                IModule rootModule = assembly.Module;
                bool rootIsExe = false;
                if (rootModule.Kind == ModuleKind.ConsoleApplication) rootIsExe = true;
                DoRTA(host, visitor, rootModule, rootIsExe);
                GenerateFacts(host, visitor);
            }
        }

        static void DoRTA(IMetadataHost host, MetadataVisitor visitor, IModule rootModule, bool rootIsExe)
        {
            RTAAnalyzer rtaAnalyzer = new RTAAnalyzer(rootIsExe);
            visitor.SetupRTAAnalyzer(rtaAnalyzer);
            Utils.SetupRTAAnalyzer(rtaAnalyzer);

            IList<string> ignorePrefix = new List<string>();
            ignorePrefix.Add("System.");
            Utils.SetupPrefixesToAvoid(ignorePrefix);
            
            int iterationCount = 0;
            bool changeInCount = true;
            int startClassCnt = 0, startMethCnt = 0;
            while (changeInCount)
            {
                System.Console.WriteLine("Starting RTA ITERATION:{0}", iterationCount);
                startClassCnt = rtaAnalyzer.classes.Count;
                startMethCnt = rtaAnalyzer.methods.Count;
                System.Console.WriteLine("Counts: classes:{0}   methods:{1}", startClassCnt, startMethCnt);
                rtaAnalyzer.moduleWorkList.Clear();
                rtaAnalyzer.visitedModules.Clear();
                rtaAnalyzer.visitedClasses.Clear();
                rtaAnalyzer.ignoredClasses.Clear();
                rtaAnalyzer.visitedMethods.Clear();
                rtaAnalyzer.moduleWorkList.Add(rootModule);
                bool isRootModule = true;
                while (rtaAnalyzer.moduleWorkList.Count > 0)
                {
                    IModule module = rtaAnalyzer.moduleWorkList.First<IModule>();
                    rtaAnalyzer.visitedModules.Add(module);
                    rtaAnalyzer.moduleWorkList.RemoveAt(0);
                    visitor.IsRootModule = isRootModule;
                    visitor.Traverse(module);
                    isRootModule = false;
                    IList<IModule> loadedModules = host.LoadedUnits.OfType<IModule>().ToList();
                    foreach (IModule lmod in loadedModules)
                    {
                        if (!rtaAnalyzer.visitedModules.Contains(lmod) && !rtaAnalyzer.moduleWorkList.Contains(lmod))
                        {
                            // System.Console.WriteLine("SRK_DBG: Adding {0}", lmod.Name);
                            rtaAnalyzer.moduleWorkList.Add(lmod);
                        }
                    }
                }
                iterationCount++;
                if (rtaAnalyzer.classes.Count == startClassCnt && rtaAnalyzer.methods.Count == startMethCnt) changeInCount = false;
            }
            classes = rtaAnalyzer.classes;
            methods = rtaAnalyzer.methods;
            types = rtaAnalyzer.types;
            modules = rtaAnalyzer.visitedModules;
            entryPtMethods = rtaAnalyzer.entryPtMethods;

            System.Console.WriteLine();
            System.Console.WriteLine();
            foreach (IMethodDefinition m in methods)
            {
                System.Console.WriteLine(m.ToString());
            }
            System.Console.WriteLine();
            System.Console.WriteLine();
            foreach (IMethodDefinition m in entryPtMethods)
            {
                System.Console.WriteLine(m.ToString());
            }
            System.Console.WriteLine();
            System.Console.WriteLine();
            foreach (ITypeDefinition m in classes)
            {
                System.Console.WriteLine(m.ToString());
            }
        }

        static void GenerateFacts(IMetadataHost host, MetadataVisitor visitor)
        {
            FactGenerator factGen = new FactGenerator(classes, methods, types, entryPtMethods);
            factGen.GenerateTypeAndMethodFacts();
            factGen.GenerateChaFacts();
            visitor.SetupRTAAnalyzer(null);
            visitor.SetupFactGenerator(factGen);
            Utils.SetupFactGenerator(factGen);
            foreach (IModule lmod in modules)
            {
                visitor.Traverse(lmod);
            }
        }
    }

}

