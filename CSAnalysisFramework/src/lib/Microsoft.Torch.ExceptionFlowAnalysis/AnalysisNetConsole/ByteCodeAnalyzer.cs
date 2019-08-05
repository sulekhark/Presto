using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Analyses;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Model;
using Microsoft.Torch.ExceptionFlowAnalysis.Common;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole
{
    public static class ByteCodeAnalyzer
    {
        static RTAAnalyzer rtaAnalyzer;
        static FactGenerator factGen;
        public static void GenerateEDBFacts(string input)
        {
            using (var host = new PeReader.DefaultHost())
            {
                Types.Initialize(host);
                var visitor = new MetadataVisitor(host, null);
                IModule rootModule = GetModule(host, input);
                bool rootIsExe = false;
                if (rootModule.Kind == ModuleKind.ConsoleApplication) rootIsExe = true;
                DoRTA(host, visitor, rootModule, rootIsExe);
                GenerateFacts(host, visitor);
            }
        }

        public static IModule GetModule(IMetadataHost host, string assemblyPath)
        {
            var assembly = new Assembly(host);
            assembly.Load(assemblyPath);
            IModule module = assembly.Module;
            return module;
        }

        static void DoRTA(IMetadataHost host, MetadataVisitor visitor, IModule rootModule, bool rootIsExe)
        {
            rtaAnalyzer = new RTAAnalyzer(rootIsExe);
            visitor.SetupRTAAnalyzer(rtaAnalyzer);
            Stubber.SetupRTAAnalyzer(rtaAnalyzer);
            IList<string> ignorePrefix = new List<string>();
            ignorePrefix.Add("System.");
            Stubber.SetupPrefixesToSuppress(ignorePrefix);
            IModule stubsModule = GetModule(host, ConfigParams.StubsPath);
            StubMap.SetupStubs(stubsModule);

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
                rtaAnalyzer.moduleWorkList.Add(stubsModule);
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

            System.Console.WriteLine();
            System.Console.WriteLine();
            foreach (IMethodDefinition m in rtaAnalyzer.methods)
            {
                System.Console.WriteLine(m.ToString());
            }
            System.Console.WriteLine();
            System.Console.WriteLine();
            foreach (IMethodDefinition m in rtaAnalyzer.entryPtMethods)
            {
                System.Console.WriteLine(m.ToString());
            }
            System.Console.WriteLine();
            System.Console.WriteLine();
            foreach (ITypeDefinition m in rtaAnalyzer.classes)
            {
                System.Console.WriteLine(m.ToString());
            }
        }

        static void GenerateFacts(IMetadataHost host, MetadataVisitor visitor)
        {
            factGen = new FactGenerator();

            factGen.classes = rtaAnalyzer.classes;
            factGen.methods = rtaAnalyzer.methods;
            factGen.types = rtaAnalyzer.types;
            factGen.entryPtMethods = rtaAnalyzer.entryPtMethods;
            factGen.addrTakenInstFlds = rtaAnalyzer.addrTakenInstFlds;
            factGen.addrTakenStatFlds = rtaAnalyzer.addrTakenStatFlds;
            factGen.addrTakenMethods = rtaAnalyzer.addrTakenMethods;
            factGen.addrTakenLocals = rtaAnalyzer.addrTakenLocals;

            factGen.GenerateTypeAndMethodFacts();
            factGen.GenerateChaFacts();
            visitor.SetupRTAAnalyzer(null);
            visitor.SetupFactGenerator(factGen);
            Stubber.SetupFactGenerator(factGen);
            foreach (IModule lmod in rtaAnalyzer.visitedModules)
            {
                visitor.Traverse(lmod);
            }
        }
    }
}

