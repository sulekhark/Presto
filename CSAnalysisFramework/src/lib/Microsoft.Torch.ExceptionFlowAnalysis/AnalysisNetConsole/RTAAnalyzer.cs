
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Model;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Values;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Instructions;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole
{
    public class RTAAnalyzer
    {
        public IList<IModule> moduleWorkList;
        public ISet<IModule> visitedModules;
        public ISet<ITypeDefinition> visitedClasses;
        public ISet<ITypeDefinition> clinitProcessedClasses;
        public ISet<ITypeDefinition> ignoredClasses;
        public ISet<IMethodDefinition> visitedMethods;
        public ISet<IMethodDefinition> ignoredMethods;
        public ISet<IMethodDefinition> entryPtMethods;
        public ISet<ITypeDefinition> allocClasses;
        public ISet<ITypeDefinition> classes;
        public ISet<IMethodDefinition> methods;
        public ISet<ITypeDefinition> types;
        public readonly bool rootIsExe;

        public readonly ISet<IFieldDefinition> addrTakenInstFlds;
        public readonly ISet<IFieldDefinition> addrTakenStatFlds;
        public readonly ISet<IVariable> addrTakenLocals;
        public readonly ISet<IMethodDefinition> addrTakenMethods;
       
        public RTAAnalyzer(bool rootIsExe)
        {
            moduleWorkList = new List<IModule>();
            visitedModules = new HashSet<IModule>();
            visitedClasses = new HashSet<ITypeDefinition>();
            clinitProcessedClasses = new HashSet<ITypeDefinition>();
            ignoredClasses = new HashSet<ITypeDefinition>();
            visitedMethods = new HashSet<IMethodDefinition>();
            ignoredMethods = new HashSet<IMethodDefinition>();
            entryPtMethods = new HashSet<IMethodDefinition>();
            allocClasses = new HashSet<ITypeDefinition>();
            classes = new HashSet<ITypeDefinition>();
            methods = new HashSet<IMethodDefinition>();
            types = new HashSet<ITypeDefinition>();
            this.rootIsExe = rootIsExe;
        }

        public void VisitMethod(MethodBody mBody, ControlFlowGraph cfg, bool isRootModule)
        {
            // Going through the instructions via cfg nodes instead of directly iterating over the instructions
            // of the methodBody becuase Phi instructions may not have been inserted in the insts of the methodBody.
            foreach (var node in cfg.Nodes)
            {
                foreach (var instruction in node.Instructions)
                {
                    // System.Console.WriteLine("{0}", instruction.ToString());
                    // System.Console.WriteLine("{0}", instruction.GetType().ToString());
                    // System.Console.WriteLine();

                    if (instruction is LoadInstruction)
                    {
                        LoadInstruction lInst = instruction as LoadInstruction;
                        IValue rhsOperand = lInst.Operand;   
                        if (rhsOperand is StaticFieldAccess)
                        {
                            StaticFieldAccess rhsAcc = rhsOperand as StaticFieldAccess;
                            IFieldReference fld = rhsAcc.Field;
                            ITypeDefinition fldType = fld.ContainingType.ResolvedType;
                            Utils.CheckAndAdd(fldType);
                        }
                        // Note: calls to static methods and instance methods appear as a StaticMethodReference
                        else if (rhsOperand is StaticMethodReference)
                        {
                            StaticMethodReference sMethAddr = rhsOperand as StaticMethodReference;
                            IMethodDefinition tgtMeth = sMethAddr.Method.ResolvedMethod;
                            ITypeDefinition containingTy = tgtMeth.ContainingTypeDefinition;
                            Utils.CheckAndAdd(containingTy);
                            Utils.CheckAndAdd(tgtMeth);
                        }
                        //Note: calls to virtual, abstract or interface methods appear as VirtualMethodReference
                        else if (rhsOperand is VirtualMethodReference)
                        {
                            VirtualMethodReference sMethAddr = rhsOperand as VirtualMethodReference;
                            IMethodDefinition tgtMeth = sMethAddr.Method.ResolvedMethod;
                            ITypeDefinition containingTy = tgtMeth.ContainingTypeDefinition;
                            Utils.CheckAndAdd(containingTy);
                            Utils.CheckAndAdd(tgtMeth);
                            ProcessVirtualInvoke(tgtMeth, containingTy, true);
                        }
                        else if (rhsOperand is Reference)
                        {
                            Reference rhsRef = rhsOperand as Reference;
                            IReferenceable refOf = rhsRef.Value;
                            if (refOf is StaticFieldAccess)
                            {
                                StaticFieldAccess refAcc = refOf as StaticFieldAccess;
                                IFieldDefinition fld = refAcc.Field.ResolvedField;
                                ITypeDefinition fldType = fld.ContainingType.ResolvedType;
                                Utils.CheckAndAdd(fldType);
                            }
                        }
                    }
                    else if (instruction is StoreInstruction)
                    {
                        StoreInstruction sInst = instruction as StoreInstruction;
                        IAssignableValue lhs = sInst.Result;
                        if (lhs is StaticFieldAccess)
                        {
                            StaticFieldAccess lhsAcc = lhs as StaticFieldAccess;
                            IFieldReference fld = lhsAcc.Field;
                            ITypeDefinition fldType = fld.ContainingType.ResolvedType;
                            Utils.CheckAndAdd(fldType);
                        }
                    }
                    else if (instruction is CreateObjectInstruction)
                    {
                        CreateObjectInstruction newObjInst = instruction as CreateObjectInstruction;
                        ITypeReference objType = newObjInst.AllocationType;
                        ITypeDefinition objTypeDef = objType.ResolvedType;
                        if (!allocClasses.Contains(objTypeDef))
                        {
                            classes.Add(objTypeDef);
                            allocClasses.Add(objTypeDef);
                        }
                    }
                    else if (instruction is CreateArrayInstruction)
                    {
                        CreateArrayInstruction newArrInst = instruction as CreateArrayInstruction;
                        ITypeReference elemType = newArrInst.ElementType;
                        ITypeDefinition elemTypeDef = elemType.ResolvedType;
                        if (!allocClasses.Contains(elemTypeDef))
                        {
                            classes.Add(elemTypeDef);
                            allocClasses.Add(elemTypeDef);
                        }
                    }
                    else if (instruction is MethodCallInstruction)
                    {
                        MethodCallInstruction invkInst = instruction as MethodCallInstruction;
                        IMethodReference callTgt = invkInst.Method;
                        IMethodDefinition callTgtDef = callTgt.ResolvedMethod;
                        ITypeDefinition declType = callTgtDef.ContainingTypeDefinition;
                        
                        if ((declType.ToString().StartsWith("System.Runtime.CompilerServices.AsyncTaskMethodBuilder")) &&
                            (callTgtDef.Name.ToString() == "Start"))
                        {
                            IGenericMethodInstanceReference genericCallTgt = callTgt as IGenericMethodInstanceReference;
                            if (genericCallTgt != null && genericCallTgt.GenericArguments.Count() == 1)
                            {
                                ITypeDefinition genericParamDefn = genericCallTgt.GenericArguments.First().ResolvedType;
                                IMethodDefinition moveNextMethod = Utils.GetMethodByName(genericParamDefn, "MoveNext");
                                Utils.CheckAndAdd(moveNextMethod);
                                Utils.CheckAndAdd(genericParamDefn);
                            }
                        }
                        else
                        {
                            Utils.CheckAndAdd(callTgtDef);
                            Utils.CheckAndAdd(declType);
                            MethodCallOperation callType = invkInst.Operation;
                            if (callType == MethodCallOperation.Virtual)
                            {
                                ProcessVirtualInvoke(callTgtDef, declType, false);
                            }
                        }
                    }
                    else
                    {
                        // System.Console.WriteLine("{0}", instruction.ToString());
                        // System.Console.WriteLine("Not currently handled: {0}", instruction.GetType().ToString());
                        // System.Console.WriteLine();
                    }
                }
            }
        }

        private void ProcessVirtualInvoke(IMethodDefinition mCallee, ITypeDefinition calleeClass, bool isAddrTaken)
        {
            bool isInterface = calleeClass.IsInterface;
            
            foreach (ITypeDefinition cl in allocClasses)
            {
                if (isInterface)
                {
                    if (Utils.ImplementsInterface(cl, calleeClass))
                    {
                        foreach (IMethodDefinition meth in cl.Methods)
                        {
                            if (Utils.SignMatch(mCallee, meth))
                            {
                                Utils.CheckAndAdd(meth);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (Utils.ExtendsClass(cl, calleeClass))
                    {
                        foreach (IMethodDefinition meth in cl.Methods)
                        {
                            if (Utils.SignMatch(mCallee, meth))
                            {
                                Utils.CheckAndAdd(meth);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
