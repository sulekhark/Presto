// Copyright (c) Edgardo Zoppi.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
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
	public class MetadataVisitor : MetadataTraverser
	{
		private readonly IMetadataHost host;
		private readonly ISourceLocationProvider sourceLocationProvider;
        private FactGenerator factGen;
        private RTAAnalyzer rtaAnalyzer;
        public bool IsRootModule { set; get; }

		public MetadataVisitor(IMetadataHost host, ISourceLocationProvider sourceLocationProvider)
		{
			this.host = host;
			this.sourceLocationProvider = sourceLocationProvider; 
        }

        public void SetupFactGenerator(FactGenerator factGen)
        {
            this.factGen = factGen;
        }

        public void SetupRTAAnalyzer(RTAAnalyzer rtaAnalyzer)
        {
            this.rtaAnalyzer = rtaAnalyzer;
        }

		public override void TraverseChildren(IMethodDefinition methodDefinition)
		{

            if (Stubber.Suppress(methodDefinition)) return;
            if (methodDefinition.IsExternal) return;
            if (methodDefinition.IsAbstract)
            {
                if (rtaAnalyzer != null) rtaAnalyzer.methods.Add(methodDefinition);
                return;
            } 

            // System.Console.WriteLine();
            // System.Console.WriteLine();
            // System.Console.WriteLine("Analyzing: {0}", methodDefinition.ToString());

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
                if (IsRootModule)
                {
                    if (rtaAnalyzer.rootIsExe)
                    {
                        // Add only the Main method as entry point
                        if (Utils.IsMainMethod(methodDefinition))
                        {
                            System.Console.WriteLine("Adding main method: {0}", methodDefinition.Name.ToString());
                            rtaAnalyzer.methods.Add(methodDefinition);
                            rtaAnalyzer.entryPtMethods.Add(methodDefinition);
                        }
                    }
                    else
                    {
                        if (methodDefinition.Visibility == TypeMemberVisibility.Public ||
                            methodDefinition.Visibility == TypeMemberVisibility.Family)
                        {
                            // Otherwise, add all public methods as entry points
                            rtaAnalyzer.methods.Add(methodDefinition);
                            rtaAnalyzer.entryPtMethods.Add(methodDefinition);
                        }
                    }
                }
                if (rtaAnalyzer.methods.Contains(methodDefinition) && !rtaAnalyzer.visitedMethods.Contains(methodDefinition))
                {
                    rtaAnalyzer.visitedMethods.Add(methodDefinition);
                    System.Console.WriteLine("SRK_DBG: Visiting method: {0}", methodDefinition.Name.ToString());
                    rtaAnalyzer.VisitMethod(methodBody, cfg, IsRootModule);
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
                    factGen.GenerateFacts(methodBody, cfg, IsRootModule);
                }
            }
        }

        public override void TraverseChildren (ITypeDefinition typeDefinition)
        {
            if (rtaAnalyzer != null)
            {
                if (IsRootModule)
                {
                    rtaAnalyzer.classes.Add(typeDefinition);
                }
                if (rtaAnalyzer.classes.Contains(typeDefinition) && !rtaAnalyzer.visitedClasses.Contains(typeDefinition))
                {
                    rtaAnalyzer.visitedClasses.Add(typeDefinition);
                    if (!typeDefinition.IsValueType || typeDefinition.IsStruct) ProcessStaticConstructors(typeDefinition);
                    System.Console.WriteLine("SRK_DBG: Visiting class: {0}",typeDefinition.ToString());
                    base.TraverseChildren(typeDefinition);
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
                    System.Console.WriteLine(typeDefinition.ToString());
                    base.TraverseChildren(typeDefinition);
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
