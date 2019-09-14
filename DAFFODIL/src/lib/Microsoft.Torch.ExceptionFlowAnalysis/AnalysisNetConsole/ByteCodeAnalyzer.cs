using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend;
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
                PdbReader pdbReader = GetPdbReader(host, input);
                visitor.SetupSrcLocProvider(pdbReader);
                bool rootIsExe = false;
                if (rootModule.Kind == ModuleKind.ConsoleApplication) rootIsExe = true;
                DoRTA(host, visitor, rootModule, rootIsExe);
                GenerateFacts(host, visitor);
            }
        }

        static PdbReader GetPdbReader(IMetadataHost host, string assemblyPath)
        {
            var pdbFileName = Path.ChangeExtension(assemblyPath, "pdb");

            if (File.Exists(pdbFileName))
            {
                using (var pdbStream = File.OpenRead(pdbFileName))
                return new PdbReader(pdbStream, host);
            }
            return null;
        }

        static IModule GetModule(IMetadataHost host, string assemblyPath)
        {
            var assembly = new Assembly(host);
            assembly.Load(assemblyPath);
            IModule module = assembly.Module;
            return module;
        }

        static void Initialize(ISet<ITypeDefinition> classesSet, ISet<ITypeDefinition> entryPtList, IModule rootModule, bool rootIsExe)
        {
            foreach (ITypeDefinition ty in rootModule.GetAllTypes())
            {
                if (ty is INamedTypeDefinition && !ty.IsGeneric && !ty.IsAbstract && ty.FullName() != "<Module>")
                {
                    if (rootIsExe)
                    {
                        if (Utils.GetMethodByName(ty, "Main") != null)
                        {
                            classesSet.Add(ty);
                            entryPtList.Add(ty);
                        }
                    }
                    else
                    {
                        classesSet.Add(ty);
                        entryPtList.Add(ty);
                    }
                }
            }
        }

        static void DoRTA(IMetadataHost host, MetadataVisitor visitor, IModule rootModule, bool rootIsExe)
        {
            StreamWriter rtaLogSW = new StreamWriter(Path.Combine(ConfigParams.LogDir, "rta_log.txt"));
            rtaAnalyzer = new RTAAnalyzer(rootIsExe, rtaLogSW);
            visitor.SetupRTAAnalyzer(rtaAnalyzer);
            Stubber.SetupRTAAnalyzer(rtaAnalyzer);
            Stubs.SetupInternFactory(host.InternFactory);
            Generics.SetupInternFactory(host.InternFactory);
            IModule stubsModule = GetModule(host, ConfigParams.StubsPath);
            Stubs.SetupStubs(stubsModule);
            Initialize(rtaAnalyzer.classes, rtaAnalyzer.entryPtClasses, rootModule, rootIsExe);

            int iterationCount = 0;
            bool changeInCount = true;
            int startClassCnt = 0, startMethCnt = 0;
            while (changeInCount)
            {
                rtaLogSW.WriteLine();
                rtaLogSW.WriteLine("Starting RTA ITERATION:{0}", iterationCount);
                startClassCnt = rtaAnalyzer.classes.Count;
                startMethCnt = rtaAnalyzer.methods.Count;
                rtaLogSW.WriteLine("Counts: classes:{0}   methods:{1}", startClassCnt, startMethCnt);
                rtaAnalyzer.classWorkList.Clear();
                rtaAnalyzer.visitedClasses.Clear();
                rtaAnalyzer.ignoredClasses.Clear();
                rtaAnalyzer.visitedMethods.Clear();
                rtaAnalyzer.ignoredMethods.Clear();
                CopyAll(rtaAnalyzer.classes, rtaAnalyzer.classWorkList);
                while (rtaAnalyzer.classWorkList.Count > 0)
                {
                    ITypeDefinition ty = rtaAnalyzer.classWorkList.First<ITypeDefinition>();
                    rtaAnalyzer.classWorkList.RemoveAt(0);
                    visitor.Traverse(ty);
                }
                iterationCount++;
                if (rtaAnalyzer.classes.Count == startClassCnt && rtaAnalyzer.methods.Count == startMethCnt) changeInCount = false;
            }
            Copy(rtaAnalyzer.allocClasses, rtaAnalyzer.classes);
            rtaLogSW.WriteLine();
            rtaLogSW.WriteLine();
            foreach (IMethodDefinition m in rtaAnalyzer.methods)
            {
                rtaLogSW.WriteLine(m.GetName());
            }
            rtaLogSW.WriteLine();
            rtaLogSW.WriteLine();
            foreach (IMethodDefinition m in rtaAnalyzer.entryPtMethods)
            {
                rtaLogSW.WriteLine(m.GetName());
            }
            rtaLogSW.WriteLine();
            rtaLogSW.WriteLine();
            foreach (ITypeDefinition m in rtaAnalyzer.classes)
            {
                rtaLogSW.WriteLine(m.FullName());
            }
            rtaLogSW.WriteLine("+++++++++++++++ RTA DONE ++++++++++++++++++");
            rtaLogSW.Close();
        }

        static void GenerateFacts(IMetadataHost host, MetadataVisitor visitor)
        {
            StreamWriter tacLogSW = new StreamWriter(Path.Combine(ConfigParams.LogDir, "tac_log.txt"));
            factGen = new FactGenerator(tacLogSW);

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
            foreach (ITypeDefinition ty in rtaAnalyzer.classes)
            {
                visitor.Traverse(ty);
            }
            factGen.CheckDomX();
            tacLogSW.Close();
        }

        static void CopyAll(ISet<ITypeDefinition> srcSet, IList<ITypeDefinition> dstList)
        {
            foreach (ITypeDefinition ty in srcSet) dstList.Add(ty);
        }

        static void Copy(ISet<ITypeDefinition> srcSet, ISet<ITypeDefinition> dstSet)
        {
            foreach (ITypeDefinition ty in srcSet) if (!dstSet.Contains(ty)) dstSet.Add(ty);
        }
    }
}

