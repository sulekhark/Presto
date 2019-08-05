
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
        public ISet<IFieldDefinition> addrTakenInstFlds;
        public ISet<IFieldDefinition> addrTakenStatFlds;
        public ISet<IVariable> addrTakenLocals;
        public ISet<IMethodDefinition> addrTakenMethods;

        public FactGenerator() {
            //Create a hypothetical field that represents all array elements
            FieldRefWrapper nullFieldRefW = new FieldRefWrapper(null);
            ProgramDoms.domF.Add(nullFieldRefW);
        }

        public void GenerateFacts(MethodBody mBody, ControlFlowGraph cfg, bool isRootModule)
        {
            MethodRefWrapper mRefW = WrapperProvider.getMethodRefW(mBody.MethodDefinition, mBody);
            System.Console.WriteLine();
            System.Console.WriteLine(mBody.MethodDefinition.Name);
            System.Console.WriteLine("==========================================");
            ProcessParams(mBody, mRefW);
            ProcessLocals(mBody, mRefW);

            // Going through the instructions via cfg nodes instead of directly iterating over the instructions
            // of the methodBody becuase Phi instructions may not have been inserted in the insts of the methodBody.
            IList<CFGNode> cfgList = Utils.getProgramFlowOrder(cfg);
            foreach (var node in cfgList)
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
                        ProcessLoadInst(lInst, mRefW);
                    }
                    else if (instruction is StoreInstruction)
                    {
                        StoreInstruction sInst = instruction as StoreInstruction;
                        ProcessStoreInst(sInst, mRefW);
                    }
                    else if (instruction is CreateObjectInstruction)
                    {
                        CreateObjectInstruction newObjInst = instruction as CreateObjectInstruction;
                        ProcessCreateObjectInst(newObjInst, mRefW, instW);
                    }
                    else if (instruction is CreateArrayInstruction)
                    {
                        CreateArrayInstruction newArrInst = instruction as CreateArrayInstruction;
                        ProcessCreateArrayInst(newArrInst, mRefW, instW);
                    }
                    else if (instruction is PhiInstruction)
                    {
                        PhiInstruction phiInst = instruction as PhiInstruction;
                        ProcessPhiInst(phiInst, mRefW);
                    }
                    else if (instruction is MethodCallInstruction)
                    {
                        MethodCallInstruction invkInst = instruction as MethodCallInstruction;
                        ProcessMethodCallInst(invkInst, mRefW, instW);
                    }
                    else if (instruction is ConvertInstruction) // cast
                    {
                        ConvertInstruction castInst = instruction as ConvertInstruction;
                        ProcessConvertInst(castInst, mRefW);
                    }
                    else if (instruction is ReturnInstruction)
                    {
                        ReturnInstruction retInst = instruction as ReturnInstruction;
                        ProcessRetInst(retInst, mRefW);
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
                if (addrTakenMethods.Contains(meth))
                {
                    AddressWrapper mAddrW = WrapperProvider.getAddrW(meth);
                    ProgramDoms.domX.Add(mAddrW);
                    ProgramRels.relAddrOfMX.Add(methW, mAddrW);
                }
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
                    if (fld.IsStatic)
                    {
                        ProgramRels.relStaticTF.Add(tyW, fldW);
                        if (addrTakenStatFlds.Contains(fld))
                        {
                            AddressWrapper fldAddrW = WrapperProvider.getAddrW(fld);
                            ProgramDoms.domX.Add(fldAddrW);
                            ProgramRels.relAddrOfFX.Add(fldW, fldAddrW);
                        }
                    }
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

        void ProcessParams(MethodBody mBody, MethodRefWrapper mRefW)
        {
            IList<IVariable> paramList = mBody.Parameters;
            int paramNdx = 0;
            foreach (IVariable param in paramList)
            {
                if (!param.Type.IsValueType || param.Type.ResolvedType.IsStruct)
                {
                    VariableWrapper paramW = WrapperProvider.getVarW(param);
                    ProgramDoms.domV.Add(paramW);
                    ProgramRels.relMmethArg.Add(mRefW, paramNdx, paramW);
                    ITypeReference varTypeRef = param.Type;
                    TypeRefWrapper varTypeRefW = WrapperProvider.getTypeRefW(varTypeRef.ResolvedType);
                    ProgramRels.relVT.Add(paramW, varTypeRefW);
                    if (param.Type.ResolvedType.IsStruct) ProgramRels.relStructV.Add(paramW);

                    if (addrTakenLocals.Contains(param))
                    {
                        AddressWrapper varAddrW = WrapperProvider.getAddrW(param);
                        ProgramDoms.domX.Add(varAddrW);
                        ProgramRels.relAddrOfVX.Add(paramW, varAddrW);
                    }
                }
                paramNdx++;
            }
            return;
        }

        void ProcessLocals(MethodBody mBody, MethodRefWrapper mRefW)
        {
            ISet<IVariable> localVarSet = mBody.Variables;
            foreach (IVariable lclVar in localVarSet)
            {
                if (!lclVar.Type.IsValueType || lclVar.Type.ResolvedType.IsStruct)
                {
                    VariableWrapper lclW = WrapperProvider.getVarW(lclVar);
                    ProgramDoms.domV.Add(lclW);
                    ITypeReference varTypeRef = lclVar.Type;
                    TypeRefWrapper varTypeRefW = WrapperProvider.getTypeRefW(varTypeRef.ResolvedType);
                    ProgramRels.relVT.Add(lclW, varTypeRefW);
                    if (lclVar.Type.ResolvedType.IsStruct) ProgramRels.relStructV.Add(lclW);

                    if (addrTakenLocals.Contains(lclVar))
                    {
                        AddressWrapper varAddrW = WrapperProvider.getAddrW(lclVar);
                        ProgramDoms.domX.Add(varAddrW);
                        ProgramRels.relAddrOfVX.Add(lclW, varAddrW);
                    }
                }
            }
            return;
        }

        void ProcessLoadInst(LoadInstruction lInst, MethodRefWrapper mRefW)
        {
            if (!lInst.Result.Type.IsValueType)
            {
                ProcessLoad(lInst, mRefW, false);
            }
            else
            {
                if (lInst.Result.Type.ResolvedType.IsStruct)
                {
                    ProcessLoad(lInst, mRefW, true);
                }
            }
            return;
        }

        void ProcessLoad(LoadInstruction lInst, MethodRefWrapper mRefW, bool isStruct)
        {
            IVariable lhsVar = lInst.Result;
            VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
            IValue rhsOperand = lInst.Operand;
            if (rhsOperand is IVariable)
            {
                IVariable rhsVar = rhsOperand as IVariable;
                VariableWrapper rhsW = WrapperProvider.getVarW(rhsVar);
                bool success = isStruct ? ProgramRels.relMStrMove.Add(mRefW, lhsW, rhsW) :
                                          ProgramRels.relMMove.Add(mRefW, lhsW, rhsW);
            }
            else if (rhsOperand is InstanceFieldAccess)
            {
                InstanceFieldAccess rhsAcc = rhsOperand as InstanceFieldAccess;
                IVariable rhsVar = rhsAcc.Instance;
                VariableWrapper rhsW = WrapperProvider.getVarW(rhsVar);
                IFieldDefinition fld = rhsAcc.Field.ResolvedField;
                FieldRefWrapper fldW = WrapperProvider.getFieldRefW(fld);
                bool success = isStruct ? ProgramRels.relMStrInstFldRead.Add(mRefW, lhsW, rhsW, fldW):
                                          ProgramRels.relMInstFldRead.Add(mRefW, lhsW, rhsW, fldW);
            }
            else if (rhsOperand is StaticFieldAccess)
            {
                StaticFieldAccess rhsAcc = rhsOperand as StaticFieldAccess;
                IFieldDefinition fld = rhsAcc.Field.ResolvedField;
                FieldRefWrapper fldW = WrapperProvider.getFieldRefW(fld);
                bool success = isStruct ? ProgramRels.relMStrStatFldRead.Add(mRefW, lhsW, fldW):
                                          ProgramRels.relMStatFldRead.Add(mRefW, lhsW, fldW);
            }
            else if (rhsOperand is ArrayElementAccess)
            {
                ArrayElementAccess rhsArr = rhsOperand as ArrayElementAccess;
                IVariable arr = rhsArr.Array;
                VariableWrapper arrW = WrapperProvider.getVarW(arr);
                FieldRefWrapper arrElemRepW = ProgramDoms.domF.GetVal(0);
                bool success = isStruct ? ProgramRels.relMStrInstFldRead.Add(mRefW, lhsW, arrW, arrElemRepW):
                                          ProgramRels.relMInstFldRead.Add(mRefW, lhsW, arrW, arrElemRepW);
            }
            // Note: calls to static methods and instance methods appear as a StaticMethodReference
            else if (rhsOperand is StaticMethodReference)
            {
                StaticMethodReference sMethAddr = rhsOperand as StaticMethodReference;
                IMethodDefinition tgtMeth = sMethAddr.Method.ResolvedMethod;
                MethodRefWrapper tgtMethW = WrapperProvider.getMethodRefW(tgtMeth);
                ProgramRels.relMAddrTakenFunc.Add(mRefW, lhsW, tgtMethW);
            }
            //Note: calls to virtual, abstract or interface methods appear as VirtualMethodReference
            else if (rhsOperand is VirtualMethodReference)
            {
                VirtualMethodReference sMethAddr = rhsOperand as VirtualMethodReference;
                IMethodDefinition tgtMeth = sMethAddr.Method.ResolvedMethod;
                MethodRefWrapper tgtMethW = WrapperProvider.getMethodRefW(tgtMeth);
                ProgramRels.relMAddrTakenFunc.Add(mRefW, lhsW, tgtMethW);
            }
            else if (rhsOperand is Dereference)
            {
                Dereference rhsDeref = rhsOperand as Dereference;
                VariableWrapper rhsW = WrapperProvider.getVarW(rhsDeref.Reference);
                ProgramRels.relMDerefRight.Add(mRefW, lhsW, rhsW);
            }
            else if (rhsOperand is Reference)
            {
                Reference rhsRef = rhsOperand as Reference;
                IReferenceable refOf = rhsRef.Value;
                if (refOf is IVariable)
                {
                    IVariable refVar = refOf as IVariable;
                    VariableWrapper refW = WrapperProvider.getVarW(refVar);
                    ProgramRels.relMAddrTakenLocal.Add(mRefW, lhsW, refW);
                }
                else if (refOf is StaticFieldAccess)
                {
                    StaticFieldAccess refAcc = refOf as StaticFieldAccess;
                    IFieldDefinition fld = refAcc.Field.ResolvedField;
                    FieldRefWrapper fldW = WrapperProvider.getFieldRefW(fld);
                    ProgramRels.relMAddrTakenStatFld.Add(mRefW, lhsW, fldW); 
                }
                else if (refOf is InstanceFieldAccess)
                {
                    InstanceFieldAccess refAcc = refOf as InstanceFieldAccess;
                    IVariable refVar = refAcc.Instance;
                    VariableWrapper refW = WrapperProvider.getVarW(refVar);
                    IFieldDefinition fld = refAcc.Field.ResolvedField;
                    FieldRefWrapper fldW = WrapperProvider.getFieldRefW(fld);
                    ProgramRels.relMAddrTakenInstFld.Add(mRefW, lhsW, refW, fldW);
                }
                else if (refOf is ArrayElementAccess)
                {
                    ArrayElementAccess refArr = refOf as ArrayElementAccess;
                    IVariable arr = refArr.Array;
                    VariableWrapper arrW = WrapperProvider.getVarW(arr);
                    FieldRefWrapper arrElemRepW = ProgramDoms.domF.GetVal(0);
                    ProgramRels.relMAddrTakenInstFld.Add(mRefW, lhsW, arrW, arrElemRepW);
                }
            }
            else if (rhsOperand is Constant)
            {
                // System.Console.WriteLine("Load Constant");
            }
            else
            {
                System.Console.WriteLine("Load Inst: {0}   {1}", rhsOperand.GetType(), rhsOperand.ToString());
            }
            return;
        }

        void ProcessStoreInst(StoreInstruction sInst, MethodRefWrapper mRefW)
        {
            if (!sInst.Operand.Type.IsValueType)
            {
                ProcessStore(sInst, mRefW, false);
            }
            else
            {
                if (sInst.Result.Type.ResolvedType.IsStruct)
                {
                    ProcessStore(sInst, mRefW, true);
                }
            }
            return;
        }

        void ProcessStore(StoreInstruction sInst, MethodRefWrapper mRefW, bool isStruct)
        {
            IVariable rhsVar = sInst.Operand;
            VariableWrapper rhsW = WrapperProvider.getVarW(rhsVar);
            IAssignableValue lhs = sInst.Result;
            if (lhs is InstanceFieldAccess)
            {
                InstanceFieldAccess lhsAcc = lhs as InstanceFieldAccess;
                IVariable lhsVar = lhsAcc.Instance;
                VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
                IFieldDefinition fld = lhsAcc.Field.ResolvedField;
                FieldRefWrapper fldW = WrapperProvider.getFieldRefW(fld);
                bool success = isStruct ? ProgramRels.relMStrInstFldWrite.Add(mRefW, lhsW, fldW, rhsW):
                                          ProgramRels.relMInstFldWrite.Add(mRefW, lhsW, fldW, rhsW);
            }
            else if (lhs is StaticFieldAccess)
            {
                StaticFieldAccess lhsAcc = lhs as StaticFieldAccess;
                IFieldDefinition fld = lhsAcc.Field.ResolvedField;
                FieldRefWrapper fldW = WrapperProvider.getFieldRefW(fld);
                bool success = isStruct ? ProgramRels.relMStrStatFldWrite.Add(mRefW, fldW, rhsW):
                                          ProgramRels.relMStatFldWrite.Add(mRefW, fldW, rhsW);
            }
            else if (lhs is ArrayElementAccess)
            {
                ArrayElementAccess lhsArr = lhs as ArrayElementAccess;
                IVariable arr = lhsArr.Array;
                VariableWrapper arrW = WrapperProvider.getVarW(arr);
                FieldRefWrapper arrElemRepW = ProgramDoms.domF.GetVal(0);
                bool success = isStruct ? ProgramRels.relMStrInstFldWrite.Add(mRefW, arrW, arrElemRepW, rhsW):
                                          ProgramRels.relMInstFldWrite.Add(mRefW, arrW, arrElemRepW, rhsW);
            }
            else if (lhs is Dereference)
            {
                Dereference lhsDeref = lhs as Dereference;
                VariableWrapper lhsW = WrapperProvider.getVarW(lhsDeref.Reference);
                ProgramRels.relMDerefLeft.Add(mRefW, lhsW, rhsW);
            }
            else
            {
                System.Console.WriteLine("Store Inst: {0}   {1}", lhs.GetType(), lhs.ToString());
            }
            return;
        }

        void ProcessCreateObjectInst(CreateObjectInstruction newObjInst, MethodRefWrapper mRefW, InstructionWrapper instW)
        {
            IVariable lhsVar = newObjInst.Result;
            VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
            ITypeDefinition objTypeDef = newObjInst.AllocationType.ResolvedType;
            TypeRefWrapper objTypeW = WrapperProvider.getTypeRefW(objTypeDef);
            ProgramDoms.domH.Add(instW);
            ProgramRels.relMAlloc.Add(mRefW, lhsW, instW);
            ProgramRels.relHT.Add(instW, objTypeW);

            foreach (IFieldDefinition fld in objTypeDef.Fields)
            { 
                if (!fld.IsStatic)
                {
                    if (addrTakenInstFlds.Contains(fld))
                    {
                        FieldRefWrapper fldW = WrapperProvider.getFieldRefW(fld);
                        AddressWrapper allocfldAddrW = WrapperProvider.getAddrW(newObjInst, fld);
                        ProgramDoms.domX.Add(allocfldAddrW);
                        ProgramRels.relAddrOfHFX.Add(instW, fldW, allocfldAddrW);
                    }
                }
            }
            return;
        }

        void ProcessCreateArrayInst(CreateArrayInstruction newArrInst, MethodRefWrapper mRefW, InstructionWrapper instW)
        {
            IVariable lhsVar = newArrInst.Result;
            VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
            ITypeDefinition elemTypeDef = newArrInst.ElementType.ResolvedType;
            TypeRefWrapper elemTypeW = WrapperProvider.getTypeRefW(elemTypeDef);
            ProgramDoms.domH.Add(instW);
            ProgramRels.relMAlloc.Add(mRefW, lhsW, instW);
            ProgramRels.relHT.Add(instW, elemTypeW);
            return;
        }

        void ProcessPhiInst(PhiInstruction phiInst, MethodRefWrapper mRefW)
        {
            IVariable lhsVar = phiInst.Result;
            if (!lhsVar.Type.IsValueType || lhsVar.Type.ResolvedType.IsStruct)
            {
                VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
                ProgramDoms.domV.Add(lhsW);
                IList<IVariable> phiArgList = phiInst.Arguments;
                bool isStruct = lhsVar.Type.ResolvedType.IsStruct;
                foreach (IVariable arg in phiArgList)
                {
                    VariableWrapper argW = WrapperProvider.getVarW(arg);
                    ProgramDoms.domV.Add(argW);
                    if (isStruct)
                    {
                        bool success = ProgramRels.relMStrMove.Add(mRefW, lhsW, argW);
                    }
                    else
                    {
                        bool success = ProgramRels.relMMove.Add(mRefW, lhsW, argW);
                    }
                }
            }
            return;
        }

        void ProcessMethodCallInst(MethodCallInstruction invkInst, MethodRefWrapper mRefW, InstructionWrapper instW)
        {
            bool done = SpecialHandlingOfInvoke(invkInst, mRefW, instW);
            if (done) return;

            ProgramDoms.domI.Add(instW);
            ProgramRels.relMI.Add(mRefW, instW);
            if (invkInst.HasResult)
            {
                IVariable lhsVar = invkInst.Result;
                if (!lhsVar.Type.IsValueType || lhsVar.Type.ResolvedType.IsStruct)
                {
                    VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
                    ProgramRels.relIinvkRet.Add(instW, 0, lhsW);
                }
            }
            IMethodDefinition origTgtDef = invkInst.Method.ResolvedMethod;
            IMethodDefinition callTgtDef = Stubber.GetMethodToAnalyze(origTgtDef);
            MethodRefWrapper callTgtW = WrapperProvider.getMethodRefW(callTgtDef);
            ProgramDoms.domM.Add(callTgtW);
            IList<IVariable> invkArgs = invkInst.Arguments;
            if (invkArgs.Count > 0)
            {
                IVariable arg0 = invkArgs[0];
                if (!arg0.Type.IsValueType || arg0.Type.ResolvedType.IsStruct)
                {
                    VariableWrapper arg0W = WrapperProvider.getVarW(arg0);
                    ProgramRels.relIinvkArg0.Add(instW, arg0W);
                }
            }
            int argNdx = 0;
            foreach (IVariable arg in invkArgs)
            {
                if (!arg.Type.IsValueType || arg.Type.ResolvedType.IsStruct)
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
            return;
        }

        bool SpecialHandlingOfInvoke(MethodCallInstruction invkInst, MethodRefWrapper mRefW, InstructionWrapper instW)
        {
            IMethodReference callTgt = invkInst.Method;
            IMethodDefinition callTgtDef = callTgt.ResolvedMethod;
            ITypeDefinition declType = callTgtDef.ContainingTypeDefinition;

            if (declType.IsDelegate && callTgtDef.IsConstructor)
            {
                IList<IVariable> invkArgs = invkInst.Arguments;
                if (invkArgs.Count != 3) Console.WriteLine("WARNING: Delegate constructor invoke has args different from 3.");
                VariableWrapper delegateVarW = WrapperProvider.getVarW(invkArgs[0]);
                VariableWrapper receiverVarW = WrapperProvider.getVarW(invkArgs[1]);
                VariableWrapper funcPtrVarW = WrapperProvider.getVarW(invkArgs[2]);
                FieldRefWrapper dummyElemW = ProgramDoms.domF.GetVal(0);
                ProgramRels.relMInstFldWrite.Add(mRefW, delegateVarW, dummyElemW, receiverVarW);
                ProgramRels.relMInstFldWrite.Add(mRefW, delegateVarW, dummyElemW, funcPtrVarW);
            }
            else if (declType.IsDelegate && callTgt.Name.ToString() == "Invoke")
            {
                ProgramDoms.domI.Add(instW);
                ProgramRels.relMI.Add(mRefW, instW);
                if (invkInst.HasResult)
                {
                    IVariable lhsVar = invkInst.Result;
                    if (!lhsVar.Type.IsValueType || lhsVar.Type.ResolvedType.IsStruct)
                    {
                        VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
                        ProgramRels.relIinvkRet.Add(instW, 0, lhsW);
                    }
                }
                IList<IVariable> invkArgs = invkInst.Arguments;
                VariableWrapper delegateVarW = WrapperProvider.getVarW(invkArgs[0]);
                ProgramRels.relDelegateIV.Add(instW, delegateVarW);   
                int argNdx = 0;
                for (int i = 1; i < invkArgs.Count; i++)
                {
                    IVariable arg = invkArgs[i];
                    if (!arg.Type.IsValueType || arg.Type.ResolvedType.IsStruct)
                    {
                        VariableWrapper argW = WrapperProvider.getVarW(arg);
                        ProgramRels.relIinvkArg.Add(instW, argNdx, argW);
                    }
                    argNdx++;
                }
            }
            else if (declType.IsDelegate && callTgt.Name.ToString() == "Combine")
            {
                if (!invkInst.HasResult) Console.WriteLine("WARNING: Delegate Combine function has no return register.");
                VariableWrapper lhsVarW = WrapperProvider.getVarW(invkInst.Result);
                IList<IVariable> invkArgs = invkInst.Arguments;
                if (invkArgs.Count != 2) Console.WriteLine("WARNING: Delegate Combine has args different from 2.");
                VariableWrapper rhsVarW1 = WrapperProvider.getVarW(invkArgs[0]);
                VariableWrapper rhsVarW2 = WrapperProvider.getVarW(invkArgs[1]);
                ProgramRels.relMMove.Add(mRefW, lhsVarW, rhsVarW1);
                ProgramRels.relMMove.Add(mRefW, lhsVarW, rhsVarW2);
            }
            else if ((declType.ToString().StartsWith("System.Runtime.CompilerServices.AsyncTaskMethodBuilder")) &&
                (callTgtDef.Name.ToString() == "Start"))
            {
                IGenericMethodInstanceReference genericCallTgt = callTgt as IGenericMethodInstanceReference;
                if (genericCallTgt != null && genericCallTgt.GenericArguments.Count() == 1)
                {
                    ITypeDefinition genericParamDefn = genericCallTgt.GenericArguments.First().ResolvedType;
                    IMethodDefinition moveNextMethod = Utils.GetMethodByName(genericParamDefn, "MoveNext");
                    callTgtDef = moveNextMethod;
                    declType = genericParamDefn;
                }
            }
            return true;
        }

        void ProcessConvertInst(ConvertInstruction castInst, MethodRefWrapper mRefW)
        {
            IVariable lhsVar = castInst.Result;
            if (!lhsVar.Type.IsValueType || lhsVar.Type.ResolvedType.IsStruct)
            {
                VariableWrapper lhsW = WrapperProvider.getVarW(lhsVar);
                IVariable rhsVar = castInst.Operand;
                VariableWrapper rhsW = WrapperProvider.getVarW(rhsVar);
                if (lhsVar.Type.ResolvedType.IsStruct)
                {
                    bool success = ProgramRels.relMStrMove.Add(mRefW, lhsW, rhsW);
                }
                else
                {
                    bool success = ProgramRels.relMMove.Add(mRefW, lhsW, rhsW);
                }
            }
            return;
        }

        void ProcessRetInst(ReturnInstruction retInst, MethodRefWrapper mRefW)
        {
            IVariable retVar = retInst.Operand;
            if (retVar != null && (!retVar.Type.IsValueType || retVar.Type.ResolvedType.IsStruct))
            {
                VariableWrapper retW = WrapperProvider.getVarW(retVar);
                ProgramRels.relMmethRet.Add(mRefW, 0, retW);
            }
            return;
        }
    }
}
