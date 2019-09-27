using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Analyses;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Serialization;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Transformations;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Model;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole
{
	public class MetadataVisitor
	{
		private readonly IMetadataHost host;
        private FactGenerator factGen;
        private RTAAnalyzer rtaAnalyzer;
        public static readonly IDictionary<IMethodDefinition, ISet<IMethodDefinition>> genericMethodMap;
        public static IDictionary<IModule, ISourceLocationProvider> moduleToPdbMap;

        static MetadataVisitor()
        {
            MethodReferenceDefinitionComparer mdc = MethodReferenceDefinitionComparer.Default;
            TypeDefinitionComparer tdc = new TypeDefinitionComparer();
            genericMethodMap = new Dictionary<IMethodDefinition, ISet<IMethodDefinition>>(mdc);
            moduleToPdbMap = new Dictionary<IModule, ISourceLocationProvider>();
        }

        public MetadataVisitor(IMetadataHost host)
		{
			this.host = host;
        }

        public void SetupSrcLocProviders(ISet<ITypeDefinition> classes)
        {
            List<IModule> moduleList = host.LoadedUnits.OfType<IModule>().ToList();
            foreach (IModule module in moduleList)
            {
                if (!(module == null || module == Dummy.Module || module == Dummy.Assembly))
                {
                    moduleToPdbMap[module] = GetPdbReader(module.Location);
                }
            }
        }
           

        public void SetupFactGenerator(FactGenerator factGen)
        {
            this.factGen = factGen;
        }

        public void SetupRTAAnalyzer(RTAAnalyzer rtaAnalyzer)
        {
            this.rtaAnalyzer = rtaAnalyzer;
        }

        PdbReader GetPdbReader(string assemblyPath)
        {
            var pdbFileName = Path.ChangeExtension(assemblyPath, "pdb");
            if (File.Exists(pdbFileName))
            {
                using (var pdbStream = File.OpenRead(pdbFileName))
                    return new PdbReader(pdbStream, host);
            }
            return null;
        }

        public void Traverse(IMethodDefinition methodDefinition)
		{
            // System.Console.WriteLine("Traversing: {0}", methodDefinition.GetName());
            if (Stubber.Suppress(methodDefinition)) return;
            if (methodDefinition.IsExternal) return;
            if (methodDefinition.IsAbstract) return;

            ITypeDefinition containingDefn = methodDefinition.ContainingTypeDefinition;
            ISourceLocationProvider sourceLocationProvider = null;
            if (containingDefn != null)
            {
                IModule mod = TypeHelper.GetDefiningUnit(containingDefn) as IModule;
                if (moduleToPdbMap.ContainsKey(mod)) sourceLocationProvider = moduleToPdbMap[mod];
            }
            var disassembler = new Disassembler(host, methodDefinition, sourceLocationProvider);
			var methodBody = disassembler.Execute();

			var cfAnalysis = new ControlFlowAnalysis(methodBody);
			// var cfg = cfAnalysis.GenerateNormalControlFlow();
			var cfg = cfAnalysis.GenerateExceptionalControlFlow();

            var domAnalysis = new DominanceAnalysis(cfg);
			domAnalysis.Analyze();
			domAnalysis.GenerateDominanceTree();

			var loopAnalysis = new NaturalLoopAnalysis(cfg);
			loopAnalysis.Analyze();

			var domFrontierAnalysis = new DominanceFrontierAnalysis(cfg);
			domFrontierAnalysis.Analyze();

			var splitter = new WebAnalysis(cfg, methodDefinition);
			splitter.Analyze();
			splitter.Transform();

			methodBody.UpdateVariables();

			var typeAnalysis = new TypeInferenceAnalysis(cfg, methodDefinition.Type);
			typeAnalysis.Analyze();

			var forwardCopyAnalysis = new ForwardCopyPropagationAnalysis(cfg);
			forwardCopyAnalysis.Analyze();
			forwardCopyAnalysis.Transform(methodBody);

            // backwardCopyAnalysis is buggy - it says so in the source file - see notes in src/test
			// var backwardCopyAnalysis = new BackwardCopyPropagationAnalysis(cfg);
			// backwardCopyAnalysis.Analyze();
			// backwardCopyAnalysis.Transform(methodBody);

			var liveVariables = new LiveVariablesAnalysis(cfg);
			liveVariables.Analyze();

			var ssa = new StaticSingleAssignment(methodBody, cfg);
			ssa.Transform();
			ssa.Prune(liveVariables);

			methodBody.UpdateVariables();

            if (rtaAnalyzer != null)
            {
                if (rtaAnalyzer.entryPtClasses.Contains(methodDefinition.ContainingTypeDefinition))
                {
                    if (rtaAnalyzer.rootIsExe)
                    {
                        // Add only the Main method as entry point
                        if (Utils.IsMainMethod(methodDefinition))
                        {
                            rtaAnalyzer.rtaLogSW.WriteLine("Adding main method: {0}", methodDefinition.GetName());
                            IMethodDefinition addedMeth = Stubber.CheckAndAdd(methodDefinition);
                            // The assumption is that addedMeth is not a template method. Here it is safe because it holds for the main method.
                            if (addedMeth != null) rtaAnalyzer.entryPtMethods.Add(addedMeth);
                        }
                    }
                    else
                    {
                        if (methodDefinition.Visibility == TypeMemberVisibility.Public ||
                            methodDefinition.Visibility == TypeMemberVisibility.Family)
                        {
                            // Otherwise, add all public methods as entry points
                            IMethodDefinition addedMeth = Stubber.CheckAndAdd(methodDefinition);
                            rtaAnalyzer.rtaLogSW.WriteLine("Adding method: {0}", methodDefinition.GetName());
                            // The assumption here is that addedMeth is not a template method.
                            // TODO: It may be the case that this assumption does not hold in some cases.
                            if (addedMeth != null) rtaAnalyzer.entryPtMethods.Add(addedMeth);
                        }
                    }
                }
                if (rtaAnalyzer.methods.Contains(methodDefinition) && !rtaAnalyzer.visitedMethods.Contains(methodDefinition))
                {
                    rtaAnalyzer.visitedMethods.Add(methodDefinition);
                    // rtaAnalyzer.rtaLogSW.WriteLine("SRK_DBG: Visiting method: {0}", methodDefinition.GetName());
                    rtaAnalyzer.VisitMethod(methodBody, cfg);
                }
                else
                {
                    rtaAnalyzer.ignoredMethods.Add(methodDefinition);
                }
                
            }
            else if (factGen != null)
            {
                if (factGen.methods.Contains(methodDefinition))
                {
                    factGen.GenerateFacts(methodBody, cfg);
                }
            }
        }

        public void Traverse (ITypeDefinition typeDefinition)
        {
            if (typeDefinition.InternedKey == 0) return; // Ignore Cci's Dummy type
            if (rtaAnalyzer != null)
            {
                if (rtaAnalyzer.classes.Contains(typeDefinition) && !rtaAnalyzer.visitedClasses.Contains(typeDefinition))
                {
                    rtaAnalyzer.visitedClasses.Add(typeDefinition);
                    if (!typeDefinition.IsValueType || typeDefinition.IsStruct) ProcessStaticConstructors(typeDefinition);
                    // rtaAnalyzer.rtaLogSW.WriteLine("SRK_DBG: Visiting class: {0}",typeDefinition.FullName());
                    TraverseMethods(typeDefinition);
                }
                else
                {
                    rtaAnalyzer.ignoredClasses.Add(typeDefinition);
                }
            }
            else if (factGen != null)
            {
                if (factGen.classes.Contains(typeDefinition))
                {
                    factGen.tacLogSW.WriteLine("FactGeneration: Traversing class: {0}", typeDefinition.FullName());
                    TraverseMethods(typeDefinition);
                }
            }
        }

        private void TraverseMethods(ITypeDefinition ty)
        {
            foreach(IMethodDefinition meth in ty.Methods)
            {
                if (meth.IsGeneric)
                {
                    if (genericMethodMap.ContainsKey(meth))
                    {
                        ISet<IMethodDefinition> instantiatedMeths = genericMethodMap[meth];
                        foreach (IMethodDefinition instMeth in instantiatedMeths) Traverse(instMeth);
                    }
                }
                else
                {
                    Traverse(meth);
                }
            }
        }

        private void ProcessStaticConstructors(ITypeDefinition ty)
        {
            if (!rtaAnalyzer.clinitProcessedClasses.Contains(ty) && !Stubber.Suppress(ty))
            {
                rtaAnalyzer.clinitProcessedClasses.Add(ty);
                IMethodDefinition clinitMeth = Utils.GetStaticConstructor(ty);
                if (clinitMeth != null) Stubber.CheckAndAdd(clinitMeth);
                if (ty.BaseClasses.Any())
                {
                    ITypeDefinition baseTy = ty.BaseClasses.Single().ResolvedType;
                    ProcessStaticConstructors(baseTy);
                }
                foreach(ITypeReference itf in ty.Interfaces)
                {
                    ITypeDefinition itfD = itf.ResolvedType;
                    ProcessStaticConstructors(itfD);
                }
            }
        }
    }
}
