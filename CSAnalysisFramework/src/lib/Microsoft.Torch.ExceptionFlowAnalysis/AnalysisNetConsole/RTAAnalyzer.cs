
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
        public IList<ITypeDefinition> classWorkList;
        public ISet<ITypeDefinition> entryPtClasses;
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
            TypeDefinitionComparer tdc = new TypeDefinitionComparer();
            MethodReferenceDefinitionComparer mdc = MethodReferenceDefinitionComparer.Default;
            FieldReferenceComparer frc = new FieldReferenceComparer();
            VariableComparer vc = new VariableComparer();

            classWorkList = new List<ITypeDefinition>();
            entryPtClasses = new HashSet<ITypeDefinition>(tdc);
            visitedClasses = new HashSet<ITypeDefinition>(tdc);
            clinitProcessedClasses = new HashSet<ITypeDefinition>(tdc);
            ignoredClasses = new HashSet<ITypeDefinition>(tdc);
            visitedMethods = new HashSet<IMethodDefinition>(mdc);
            ignoredMethods = new HashSet<IMethodDefinition>(mdc);
            entryPtMethods = new HashSet<IMethodDefinition>(mdc);
            allocClasses = new HashSet<ITypeDefinition>(tdc);
            classes = new HashSet<ITypeDefinition>(tdc);
            methods = new HashSet<IMethodDefinition>(mdc);
            types = new HashSet<ITypeDefinition>(tdc);
            this.rootIsExe = rootIsExe;

            addrTakenInstFlds = new HashSet<IFieldDefinition>(frc);
            addrTakenStatFlds = new HashSet<IFieldDefinition>(frc);
            addrTakenLocals = new HashSet<IVariable>(vc);
            addrTakenMethods = new HashSet<IMethodDefinition>(mdc);
        }

        public void VisitMethod(MethodBody mBody, ControlFlowGraph cfg)
        {
            // Going through the instructions via cfg nodes instead of directly iterating over the instructions
            // of the methodBody becuase Phi instructions may not have been inserted in the insts of the methodBody.
            foreach (var node in cfg.Nodes)
            {
                foreach (var instruction in node.Instructions)
                {
                    // System.Console.WriteLine("{0}", instruction.ToString());
                    // System.Console.WriteLine("{0}", instruction.GetType().FullName());
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
                            Stubber.CheckAndAdd(fldType);
                        }
                        // Note: calls to static methods and instance methods appear as a StaticMethodReference
                        else if (rhsOperand is StaticMethodReference)
                        {
                            StaticMethodReference sMethAddr = rhsOperand as StaticMethodReference;
                            IMethodDefinition tgtMeth = sMethAddr.Method.ResolvedMethod;
                            ITypeDefinition containingTy = tgtMeth.ContainingTypeDefinition;
                            Stubber.CheckAndAdd(containingTy);
                            IMethodDefinition addedMeth = Stubber.CheckAndAdd(tgtMeth);
                            // addrTakenMethods do not contain templates.
                            if (addedMeth != null) addrTakenMethods.Add(addedMeth);
                        }
                        //Note: calls to virtual, abstract or interface methods appear as VirtualMethodReference
                        else if (rhsOperand is VirtualMethodReference)
                        {
                            VirtualMethodReference sMethAddr = rhsOperand as VirtualMethodReference;
                            IMethodDefinition tgtMeth = sMethAddr.Method.ResolvedMethod;
                            ITypeDefinition containingTy = tgtMeth.ContainingTypeDefinition;
                            ITypeDefinition addedTy = Stubber.CheckAndAdd(containingTy);
                            IMethodDefinition addedMeth = Stubber.CheckAndAdd(tgtMeth);
                            if (addedTy != null && addedMeth != null)
                            {
                                // addrTakenMethods do not contain templates.
                                addrTakenMethods.Add(addedMeth);
                                ProcessVirtualInvoke(addedMeth, addedTy, true);
                            }
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
                                Stubber.CheckAndAdd(fldType);
                                addrTakenStatFlds.Add(fld);
                            }
                            else if (refOf is IVariable)
                            {
                                IVariable refVar = refOf as IVariable;
                                if (!refVar.Type.IsValueType || refVar.Type.ResolvedType.IsStruct) addrTakenLocals.Add(refVar);
                            }
                            else if (refOf is InstanceFieldAccess)
                            {
                                InstanceFieldAccess refAcc = refOf as InstanceFieldAccess;
                                IFieldDefinition fld = refAcc.Field.ResolvedField;
                                addrTakenInstFlds.Add(fld);
                            }
                            else if (refOf is ArrayElementAccess)
                            {
                                // All arrays will be added into domX as potential address taken.
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
                            Stubber.CheckAndAdd(fldType);
                        }
                    }
                    else if (instruction is CreateObjectInstruction)
                    {
                        CreateObjectInstruction newObjInst = instruction as CreateObjectInstruction;
                        ITypeReference objType = newObjInst.AllocationType;
                        ITypeDefinition objTypeDef = objType.ResolvedType;
                        if (objTypeDef is IGenericTypeInstance) objTypeDef = objTypeDef.ResolvedType;
                        ITypeDefinition addedTy = Stubber.CheckAndAdd(objTypeDef);
                        if (addedTy != null && !allocClasses.Contains(addedTy))
                        {
                            allocClasses.Add(addedTy);
                        }
                        else if (addedTy == null)
                        // addedTy will be null for non-stubbed System.* types.
                        // We don't want to analyze methods of such types, but we still want to track such objects.
                        {
                            allocClasses.Add(objTypeDef);
                        }
                    }
                    else if (instruction is CreateArrayInstruction)
                    {
                        CreateArrayInstruction newArrInst = instruction as CreateArrayInstruction;
                        ITypeReference elemType = newArrInst.ElementType;
                        ITypeDefinition elemTypeDef = elemType.ResolvedType;
                        ITypeDefinition addedTy = Stubber.CheckAndAdd(elemTypeDef);
                        if (addedTy != null && !allocClasses.Contains(addedTy))
                        {
                            allocClasses.Add(addedTy);
                        }
                        else if (addedTy == null)
                        // addedTy will be null for non-stubbed System.* types.
                        // We don't want to analyze methods of such types, but we still want to track such objects.
                        {
                            allocClasses.Add(elemTypeDef);
                        }
                    }
                    else if (instruction is MethodCallInstruction)
                    {
                        MethodCallInstruction invkInst = instruction as MethodCallInstruction;
                        IMethodReference callTgt = invkInst.Method;
                        IMethodDefinition callTgtDef = callTgt.ResolvedMethod;
                        ITypeDefinition declType = callTgtDef.ContainingTypeDefinition;
                        ITypeDefinition addedType = Stubber.CheckAndAdd(declType);
                        IMethodDefinition addedMeth = Stubber.CheckAndAdd(callTgtDef);
                        MethodCallOperation callType = invkInst.Operation;
                        if (callType == MethodCallOperation.Virtual && addedType != null && addedMeth != null)
                        {
                            ProcessVirtualInvoke(addedMeth, addedType, false);
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

        public void ProcessVirtualInvoke(IMethodDefinition mCallee, ITypeDefinition calleeClass, bool isAddrTaken)
        {
            // mCallee is an ordinary method - never a template method.
            // calleeClass and mCallee are either both stubbed or, both unstubbed - i.e. they are consistent.
            bool isInterface = calleeClass.IsInterface;
            
            foreach (ITypeDefinition cl in allocClasses)
            {
                if (!Stubber.Suppress(cl))
                {
                    bool process = false;
                    if (isInterface && Utils.ImplementsInterface(cl, calleeClass))
                        process = true;
                    if (Utils.ExtendsClass(cl, calleeClass))
                        process = true;
                    if (!process) continue;
                    foreach (IMethodDefinition meth in cl.Methods)
                    {
                        if (meth is IGenericMethodInstance)
                        {
                            if (Utils.GenericStubMatch(mCallee, meth))
                            {
                                IMethodDefinition instMeth = Generics.GetInstantiatedMeth(meth, mCallee);
                                IMethodDefinition addedMeth = Stubber.CheckAndAdd(instMeth);
                                if (addedMeth != null && isAddrTaken) addrTakenMethods.Add(addedMeth);
                                break;
                            }
                        }
                        else
                        {
                            if (Utils.StubMatch(mCallee, meth))
                            {
                                IMethodDefinition addedMeth = Stubber.CheckAndAdd(meth);
                                if (addedMeth != null && isAddrTaken) addrTakenMethods.Add(addedMeth);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
