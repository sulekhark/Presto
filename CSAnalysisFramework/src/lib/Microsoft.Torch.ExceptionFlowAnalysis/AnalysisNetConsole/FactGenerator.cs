
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Model;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Values;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Instructions;
using Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts;
using Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Relations;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole
{
    public class FactGenerator
    {
        public ISet<ITypeDefinition> classes;
        public ISet<ITypeDefinition> types;
        public ISet<IMethodDefinition> methods;
        public ISet<IMethodDefinition> entryPtMethods;
        public FactGenerator(ISet<ITypeDefinition> classes, ISet<IMethodDefinition> methods, ISet<ITypeDefinition> types,
                             ISet<IMethodDefinition> entryPtM) {
            //Create a hypothetical field that represents all array elements
            FieldRefWrapper nullFieldRefW = new FieldRefWrapper(null);
            ProgramDoms.domF.Add(nullFieldRefW);
            this.classes = classes;
            this.methods = methods;
            this.types = types;
            this.entryPtMethods = entryPtM;
        }

        public void GenerateFacts(MethodBody mBody, ControlFlowGraph cfg, bool isRootModule)
        {
            MethodRefWrapper mRefW = WrapperProvider.getMethodRefW(mBody.MethodDefinition, mBody);
            System.Console.WriteLine();
            System.Console.WriteLine(mBody.MethodDefinition.Name);
            System.Console.WriteLine("==========================================");
            IList<IVariable> paramList = mBody.Parameters;
            int paramNdx = 0;
            foreach (IVariable param in paramList)
            {
                if (!param.Type.IsValueType)
                {
                    VariableWrapper paramW = WrapperProvider.getVarW(param);
                    ProgramDoms.domV.Add(paramW);
                    ProgramRels.relMmethArg.Add(mRefW, paramNdx, paramW);
                    ITypeReference varTypeRef = param.Type;
                    TypeRefWrapper varTypeRefW = WrapperProvider.getTypeRefW(varTypeRef.ResolvedType);
                    ProgramRels.relVT.Add(paramW, varTypeRefW);
                }
                paramNdx++;
            }

            ISet<IVariable> localVarSet = mBody.Variables;
            foreach (IVariable lclVar in localVarSet)
            {
                if (!lclVar.Type.IsValueType)
                {
                    VariableWrapper lclW = WrapperProvider.getVarW(lclVar);
                    ProgramDoms.domV.Add(lclW);
                    ITypeReference varTypeRef = lclVar.Type;
                    TypeRefWrapper varTypeRefW = WrapperProvider.getTypeRefW(varTypeRef.ResolvedType);
                    ProgramRels.relVT.Add(lclW, varTypeRefW);
                }
            }
            // Going through the instructions via cfg nodes instead of directly iterating over the instructions
            // of the methodBody becuase Phi instructions may not have been inserted in the insts of the methodBody.
            foreach (var node in cfg.Nodes)
            {
                foreach (var instruction in node.Instructions)
                {
                    System.Console.WriteLine("{0}", instruction.ToString());
                    // System.Console.WriteLine("{0}", instruction.GetType().ToString());
                    // System.Console.WriteLine();
                    InstructionWrapper instW = WrapperProvider.getInstW(instruction);
                    ProgramDoms.domP.Add(instW);

                    if (instruction is LoadInstruction)
                    {
                        LoadInstruction lInst = instruction as LoadInstruction;
                        IVariable lhsVar = lInst.Result;
                        if (!lhsVar.Type.IsValueType)
                        {
                            VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
                            IValue rhsOperand = lInst.Operand;
                            if (rhsOperand is IVariable)
                            {
                                IVariable rhsVar = rhsOperand as IVariable;
                                VariableWrapper rhsW = WrapperProvider.getVarW(rhsVar);
                                bool success = ProgramRels.relMMove.Add(mRefW, lhsW, rhsW);
                            }
                            else if (rhsOperand is InstanceFieldAccess)
                            {
                                InstanceFieldAccess rhsAcc = rhsOperand as InstanceFieldAccess;
                                IVariable rhsVar = rhsAcc.Instance;
                                VariableWrapper rhsW = WrapperProvider.getVarW(rhsVar);
                                IFieldDefinition fld = rhsAcc.Field.ResolvedField;
                                FieldRefWrapper fldW = WrapperProvider.getFieldRefW(fld);
                                bool success = ProgramRels.relMInstFldRead.Add(mRefW, lhsW, rhsW, fldW);
                            }
                            else if (rhsOperand is StaticFieldAccess)
                            {
                                StaticFieldAccess rhsAcc = rhsOperand as StaticFieldAccess;
                                IFieldDefinition fld = rhsAcc.Field.ResolvedField;
                                FieldRefWrapper fldW = WrapperProvider.getFieldRefW(fld);
                                bool success = ProgramRels.relMStatFldRead.Add(mRefW, lhsW, fldW);
                            }
                            else if (rhsOperand is ArrayElementAccess)
                            {
                                ArrayElementAccess rhsArr = rhsOperand as ArrayElementAccess;
                                IVariable arr = rhsArr.Array;
                                VariableWrapper arrW = WrapperProvider.getVarW(arr);
                                FieldRefWrapper arrElemRepW = ProgramDoms.domF.GetVal(0);
                                bool success = ProgramRels.relMInstFldRead.Add(mRefW, lhsW, arrW, arrElemRepW);
                            }
                            else if (rhsOperand is Constant)
                            {
                                // System.Console.WriteLine("Load Constant");
                            }
                            else
                            {
                                // System.Console.WriteLine("Load Inst: No idea: {0}   {1}", rhsOperand.Type, rhsOperand.ToString());
                            }
                        }
                    }
                    else if (instruction is StoreInstruction)
                    {
                        StoreInstruction sInst = instruction as StoreInstruction;
                        IVariable rhsVar = sInst.Operand;
                        if (!rhsVar.Type.IsValueType)
                        {
                            VariableWrapper rhsW = WrapperProvider.getVarW(rhsVar);
                            IAssignableValue lhs = sInst.Result;
                            if (lhs is InstanceFieldAccess)
                            {
                                InstanceFieldAccess lhsAcc = lhs as InstanceFieldAccess;
                                IVariable lhsVar = lhsAcc.Instance;
                                VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
                                IFieldDefinition fld = lhsAcc.Field.ResolvedField;
                                FieldRefWrapper fldW = WrapperProvider.getFieldRefW(fld);
                                bool success = ProgramRels.relMInstFldWrite.Add(mRefW, lhsW, fldW, rhsW);
                            }
                            else if (lhs is StaticFieldAccess)
                            {
                                StaticFieldAccess lhsAcc = lhs as StaticFieldAccess;
                                IFieldDefinition fld = lhsAcc.Field.ResolvedField;
                                FieldRefWrapper fldW = WrapperProvider.getFieldRefW(fld);
                                bool success = ProgramRels.relMStatFldWrite.Add(mRefW, fldW, rhsW);
                            }
                            else if (lhs is ArrayElementAccess)
                            {
                                ArrayElementAccess lhsArr = lhs as ArrayElementAccess;
                                IVariable arr = lhsArr.Array;
                                VariableWrapper arrW = WrapperProvider.getVarW(arr);
                                FieldRefWrapper arrElemRepW = ProgramDoms.domF.GetVal(0);
                                bool success = ProgramRels.relMInstFldWrite.Add(mRefW, arrW, arrElemRepW, rhsW);
                            }
                            else
                            {
                                System.Console.WriteLine("Store Inst: No idea");
                            }
                        }
                    }
                    else if (instruction is CreateObjectInstruction)
                    {
                        CreateObjectInstruction newObjInst = instruction as CreateObjectInstruction;
                        IVariable lhsVar = newObjInst.Result;
                        VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
                        ITypeDefinition objTypeDef = newObjInst.AllocationType.ResolvedType;
                        TypeRefWrapper objTypeW = WrapperProvider.getTypeRefW(objTypeDef);
                        ProgramDoms.domH.Add(instW);
                        ProgramRels.relMAlloc.Add(mRefW, lhsW, instW);
                        ProgramRels.relHT.Add(instW, objTypeW);
                    }
                    else if (instruction is CreateArrayInstruction)
                    {
                        CreateArrayInstruction newArrInst = instruction as CreateArrayInstruction;
                        IVariable lhsVar = newArrInst.Result;
                        VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
                        ITypeDefinition elemTypeDef = newArrInst.ElementType.ResolvedType;
                        TypeRefWrapper elemTypeW = WrapperProvider.getTypeRefW(elemTypeDef);
                        ProgramDoms.domH.Add(instW);
                        ProgramRels.relMAlloc.Add(mRefW, lhsW, instW);
                        ProgramRels.relHT.Add(instW, elemTypeW);
                    }
                    else if (instruction is PhiInstruction)
                    {
                        PhiInstruction phiInst = instruction as PhiInstruction;
                        IVariable lhsVar = phiInst.Result;
                        if (!lhsVar.Type.IsValueType)
                        {
                            VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
                            ProgramDoms.domV.Add(lhsW);
                            IList<IVariable> phiArgList = phiInst.Arguments;
                            foreach (IVariable arg in phiArgList)
                            {
                                VariableWrapper argW = WrapperProvider.getVarW(arg);
                                ProgramDoms.domV.Add(argW);
                                bool success = ProgramRels.relMMove.Add(mRefW, lhsW, argW);
                            }
                        }
                    }
                    else if (instruction is MethodCallInstruction)
                    {
                        ProgramDoms.domI.Add(instW);
                        ProgramRels.relMI.Add(mRefW, instW);

                        MethodCallInstruction invkInst = instruction as MethodCallInstruction;
                        if (invkInst.HasResult)
                        {
                            IVariable lhsVar = invkInst.Result;
                            if (!lhsVar.Type.IsValueType)
                            {
                                VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
                                ProgramRels.relIinvkRet.Add(instW, 0, lhsW);
                            }
                        }
                        IMethodReference callTgt = invkInst.Method;
                        IMethodDefinition callTgtDef = callTgt.ResolvedMethod;
                        ITypeDefinition declType = callTgtDef.ContainingTypeDefinition;

                        if ((declType.ToString().StartsWith("System.Runtime.CompilerServices.AsyncTaskMethodBuilder")) &&
                            (callTgtDef.Name.ToString() == "Start"))
                        {
                            IGenericMethodInstanceReference genericCallTgt = callTgt as IGenericMethodInstanceReference;
                            if (genericCallTgt != null  && genericCallTgt.GenericArguments.Count() == 1)
                            {
                                ITypeDefinition genericParamDefn = genericCallTgt.GenericArguments.First().ResolvedType;
                                IMethodDefinition moveNextMethod = Utils.GetMethodByName(genericParamDefn, "MoveNext");
                                callTgtDef = moveNextMethod;
                                declType = genericParamDefn;
                            }
                        }

                        MethodRefWrapper callTgtW = WrapperProvider.getMethodRefW(callTgtDef);
                        ProgramDoms.domM.Add(callTgtW);
                        IList<IVariable> invkArgs = invkInst.Arguments;
                        if (invkArgs.Count > 0)
                        {
                            IVariable arg0 = invkArgs[0];
                            if (!arg0.Type.IsValueType)
                            {
                                VariableWrapper arg0W = WrapperProvider.getVarW(arg0);
                                ProgramRels.relIinvkArg0.Add(instW, arg0W);
                            }
                        }
                        int argNdx = 0;
                        foreach (IVariable arg in invkArgs)
                        {
                            if (!arg.Type.IsValueType)
                            {
                                VariableWrapper argW = WrapperProvider.getVarW(arg);
                                ProgramRels.relIinvkArg.Add(instW, argNdx, argW);
                            }
                            argNdx++;
                        }
                        MethodCallOperation callType = invkInst.Operation;
                        if (callType == MethodCallOperation.Virtual)
                        {
                            ProgramRels.relVirtIM.Add(instW, callTgtW);
                        }
                        else if (callType == MethodCallOperation.Static)
                        {
                            ProgramRels.relStatIM.Add(instW, callTgtW);
                        }
                        else
                        {
                            // The only other type is MethodCallOperation.Jump which we ignore.
                        }
                    }
                    else if (instruction is ConvertInstruction) // cast
                    {
                        ConvertInstruction castInst = instruction as ConvertInstruction;
                        IVariable lhsVar = castInst.Result;
                        if (!lhsVar.Type.IsValueType)
                        {
                            VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
                            IVariable rhsVar = castInst.Operand;
                            VariableWrapper rhsW = WrapperProvider.getVarW(rhsVar);
                            bool success = ProgramRels.relMMove.Add(mRefW, lhsW, rhsW);
                        }
                    }
                    else if (instruction is ReturnInstruction)
                    {
                        ReturnInstruction retInst = instruction as ReturnInstruction;
                        IVariable retVar = retInst.Operand;
                        if (retVar != null && !retVar.Type.IsValueType)
                        {
                            VariableWrapper retW = WrapperProvider.getVarW(retVar);
                            ProgramRels.relMmethRet.Add(mRefW, 0, retW);
                        }
                    }
                    else if (instruction is InitializeObjectInstruction)
                    {
                         // Ignore
                    }
                    else if (instruction is LoadTokenInstruction)
                    {
                         // Ignore
                    }
                    else if (instruction is BranchInstruction)
                    {
                        // Ignore
                    }
                    else if (instruction is UnaryInstruction || instruction is BinaryInstruction || instruction is NopInstruction)
                    {
                        // Ignore
                    }
                    else if (instruction is BreakpointInstruction || instruction is TryInstruction ||
                             instruction is FaultInstruction || instruction is FinallyInstruction ||
                             instruction is CatchInstruction || instruction is ThrowInstruction)
                    {
                        // Ignore
                    }
                    else if (instruction is SwitchInstruction || instruction is SizeofInstruction)
                    {
                        // Ignore
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

        public void GenerateTypeAndMethodFacts()
        {
            foreach (IMethodDefinition meth in methods)
            {
                MethodRefWrapper methW = WrapperProvider.getMethodRefW(meth);
                ProgramDoms.domM.Add(methW);
                if (meth.IsStatic)
                {
                    ITypeDefinition cl = meth.ContainingTypeDefinition;
                    TypeRefWrapper clW = WrapperProvider.getTypeRefW(cl);
                    ProgramRels.relStaticTM.Add(clW, methW);
                }
                if (meth.IsStaticConstructor)
                {
                    ITypeDefinition cl = meth.ContainingTypeDefinition;
                    TypeRefWrapper clW = WrapperProvider.getTypeRefW(cl);
                    ProgramRels.relClinitTM.Add(clW, methW);
                }
            }
            foreach (IMethodDefinition meth in entryPtMethods)
            {
                MethodRefWrapper methW = WrapperProvider.getMethodRefW(meth);
                ProgramRels.relEntryPtM.Add(methW);
            }
            foreach (ITypeDefinition ty in classes)
            {
                TypeRefWrapper tyW = WrapperProvider.getTypeRefW(ty);
                ProgramDoms.domT.Add(tyW);
                if (!ty.IsInterface)
                {
                    ProgramRels.relClassT.Add(tyW);
                }
                foreach (IFieldDefinition fld in ty.Fields)
                {
                    FieldRefWrapper fldW = WrapperProvider.getFieldRefW(fld);
                    ProgramDoms.domF.Add(fldW);
                    if (fld.IsStatic) ProgramRels.relStaticTF.Add(tyW, fldW);
                }
            }
        }

        public void GenerateChaFacts()
        {
           ClassHierarchyAnalysis cha = new ClassHierarchyAnalysis();
           cha.Analyze(classes);
           foreach (ITypeReference ty in cha.Types)
           {
                TypeRefWrapper tyW = WrapperProvider.getTypeRefW(ty);
                ProgramRels.relSub.Add(tyW, tyW);
                foreach (ITypeDefinition subTy in cha.GetSubtypes(ty))
                {
                    TypeRefWrapper subTyW = WrapperProvider.getTypeRefW(subTy);
                    ProgramRels.relSub.Add(subTyW, tyW);
                }
           }

           ISet<IMethodDefinition> allInstanceMethods = new HashSet<IMethodDefinition>();
           foreach (IMethodDefinition meth in methods)
           {
                if (!meth.IsStatic) allInstanceMethods.Add(meth);
           }
           foreach (ITypeDefinition ty in classes)
           {
                TypeRefWrapper tyW = WrapperProvider.getTypeRefW(ty);
                if (ty is IArrayTypeReference)
                {
                    foreach (IMethodDefinition instMeth in allInstanceMethods)
                    {
                        MethodRefWrapper instMethW = WrapperProvider.getMethodRefW(instMeth);
                        ProgramRels.relCha.Add(instMethW, tyW, instMethW);
                    }
                    continue;
                }
                else
                {
                    bool tyIsInterface = false;
                    if (ty.IsInterface) tyIsInterface = true;
                 
                    foreach (IMethodDefinition tyMeth in ty.Methods)
                    {
                        if (tyMeth.Visibility == TypeMemberVisibility.Private) continue;
                        if (tyMeth.IsConstructor) continue;
                        if (!methods.Contains(tyMeth)) continue;
                        MethodRefWrapper tyMethW = WrapperProvider.getMethodRefW(tyMeth);
                        foreach (ITypeDefinition candidateTy in classes)
                        {
                            if (candidateTy is IArrayTypeReference) continue;
                            if (candidateTy.IsAbstract || candidateTy.IsInterface) continue;
                            TypeRefWrapper candidateTyW = WrapperProvider.getTypeRefW(candidateTy);
                            bool implementationExists = false;
                            if (tyIsInterface)
                            {
                                implementationExists = Utils.ImplementsInterface(candidateTy, ty);
                            }
                            else
                            {
                                implementationExists = Utils.ExtendsClass(candidateTy, ty);
                            }
                            if (implementationExists)
                            {
                                IMethodDefinition candidateMeth = Utils.GetSignMatchMethod(candidateTy, tyMeth);
                                if (candidateMeth != null && methods.Contains(candidateMeth))
                                {
                                    MethodRefWrapper candidateMethW = WrapperProvider.getMethodRefW(candidateMeth);
                                    ProgramRels.relCha.Add(tyMethW, candidateTyW, candidateMethW);
                                }
                            }
                        }
                    }
                }
           }
        }
    }
}
