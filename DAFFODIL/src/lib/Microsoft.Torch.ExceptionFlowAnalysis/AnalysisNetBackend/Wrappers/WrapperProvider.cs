
using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Instructions;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Values;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Model;
using Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public static class WrapperProvider
    {
        private readonly static IDictionary<IMethodReference, MethodRefWrapper> MethRefToWrapperMap;
        private readonly static IDictionary<ITypeReference, TypeRefWrapper> TypeRefToWrapperMap;
        private readonly static IDictionary<IFieldReference, FieldRefWrapper> FieldRefToWrapperMap;
        private readonly static IDictionary<Instruction, InstructionWrapper> InstToWrapperMap;
        private readonly static IDictionary<IVariable, VariableWrapper> VarToWrapperMap;

        //AddressWrapper dictionaries
        private readonly static IDictionary<IMethodReference, AddressWrapper> MethRefToAddrWrapperMap;
        private readonly static IDictionary<Instruction, IDictionary<IFieldReference, AddressWrapper>> InstFldRefToAddrWrapperMap;
        private readonly static IDictionary<IFieldReference, AddressWrapper> FieldRefToAddrWrapperMap;
        private readonly static IDictionary<IVariable, AddressWrapper> VarToAddrWrapperMap;
        private readonly static IDictionary<Instruction, AddressWrapper> ArrayToAddrWrapperMap;

        //HeapAccWrapper dictionaries
        private readonly static IDictionary<Instruction, HeapAccWrapper> InstToHeapAccWrapperMap;
        private readonly static IDictionary<IVariable, HeapAccWrapper> VarToHeapAccWrapperMap;

        //ExHandlerWrapper dictionary
        private readonly static IDictionary<Instruction, ExHandlerWrapper> InstToExHandlerWrapperMap;

        static readonly FieldReferenceComparer frc;
        static WrapperProvider()
        {
            frc = new FieldReferenceComparer();
            TypeDefinitionComparer tdc = new TypeDefinitionComparer();
            InstructionComparer idc = new InstructionComparer();
            VariableComparer vc = new VariableComparer();

            MethRefToWrapperMap = new Dictionary<IMethodReference, MethodRefWrapper>(MethodReferenceDefinitionComparer.Default);
            TypeRefToWrapperMap = new Dictionary<ITypeReference, TypeRefWrapper>(tdc);
            FieldRefToWrapperMap = new Dictionary<IFieldReference, FieldRefWrapper>(frc);
            InstToWrapperMap = new Dictionary<Instruction, InstructionWrapper>(new InstructionComparer());
            VarToWrapperMap = new Dictionary<IVariable, VariableWrapper>(vc);

            MethRefToAddrWrapperMap = new Dictionary<IMethodReference, AddressWrapper>(MethodReferenceDefinitionComparer.Default);
            InstFldRefToAddrWrapperMap = new Dictionary<Instruction, IDictionary<IFieldReference, AddressWrapper>>(idc);
            FieldRefToAddrWrapperMap = new Dictionary<IFieldReference, AddressWrapper>(frc);
            VarToAddrWrapperMap = new Dictionary<IVariable, AddressWrapper>(vc);
            ArrayToAddrWrapperMap = new Dictionary<Instruction, AddressWrapper>(idc);

            InstToHeapAccWrapperMap = new Dictionary<Instruction, HeapAccWrapper>(idc);
            VarToHeapAccWrapperMap = new Dictionary<IVariable, HeapAccWrapper>(vc);

            InstToExHandlerWrapperMap = new Dictionary<Instruction, ExHandlerWrapper>(idc);
        }

        public static MethodRefWrapper getMethodRefW (IMethodReference methRef)
        {
            if (MethRefToWrapperMap.ContainsKey(methRef))
            {
                MethodRefWrapper mRefW = MethRefToWrapperMap[methRef];
                return mRefW;
            }
            else
            {
                MethodRefWrapper mRefW = new MethodRefWrapper(methRef);
                MethRefToWrapperMap.Add(methRef, mRefW);
                return mRefW;
            }
        }

        public static MethodRefWrapper getMethodRefW(IMethodReference methRef, MethodBody methBody)
        {
            if (MethRefToWrapperMap.ContainsKey(methRef))
            {
                MethodRefWrapper mRefW = MethRefToWrapperMap[methRef];
                mRefW.Update(methBody);
                return mRefW;
            }
            else
            {
                MethodRefWrapper mRefW = new MethodRefWrapper(methRef, methBody);
                MethRefToWrapperMap.Add(methRef, mRefW);
                return mRefW;
            }
        }

        public static TypeRefWrapper getTypeRefW(ITypeReference typeRef)
        {
            if (TypeRefToWrapperMap.ContainsKey(typeRef))
            {
                return TypeRefToWrapperMap[typeRef];
            }
            else
            {
                TypeRefWrapper tRefW = new TypeRefWrapper(typeRef);
                TypeRefToWrapperMap.Add(typeRef, tRefW);
                return tRefW;
            }
        }

        public static FieldRefWrapper getFieldRefW(IFieldReference fieldRef)
        {
            if (FieldRefToWrapperMap.ContainsKey(fieldRef))
            {
                return FieldRefToWrapperMap[fieldRef];
            }
            else
            {
                FieldRefWrapper fRefW = new FieldRefWrapper(fieldRef);
                FieldRefToWrapperMap.Add(fieldRef, fRefW);
                return fRefW;
            }
        }

        public static InstructionWrapper getInstW(Instruction inst, IMethodDefinition meth)
        {
            if (InstToWrapperMap.ContainsKey(inst))
            {
                return InstToWrapperMap[inst];
            }
            else
            {
                InstructionWrapper instW = new InstructionWrapper(inst, meth);
                InstToWrapperMap.Add(inst, instW);
                return instW;
            }
        }

        public static VariableWrapper getVarW(IVariable var)
        {
            if (VarToWrapperMap.ContainsKey(var))
            {
                return VarToWrapperMap[var];
            }
            else
            {
                VariableWrapper varW = new VariableWrapper(var);
                VarToWrapperMap.Add(var, varW);
                return varW;
            }
        }
 
        public static AddressWrapper getAddrW(IMethodReference mRef)
        {
            if (MethRefToAddrWrapperMap.ContainsKey(mRef))
            {
                return MethRefToAddrWrapperMap[mRef];
            }
            else
            {
                MethodRefWrapper mRefW = getMethodRefW(mRef);
                AddressWrapper methAW = new AddressWrapper(mRefW);
                MethRefToAddrWrapperMap[mRef] = methAW;
                return methAW;
            }
        }

        public static AddressWrapper getAddrW(Instruction inst, IFieldReference fldRef, IMethodDefinition meth)
        {
            if (InstFldRefToAddrWrapperMap.ContainsKey(inst))
            {
                IDictionary<IFieldReference, AddressWrapper> innerDict = InstFldRefToAddrWrapperMap[inst];

                if (innerDict != null && innerDict.ContainsKey(fldRef))
                {
                    return innerDict[fldRef];
                }
                if (innerDict == null)
                {
                    InstFldRefToAddrWrapperMap[inst] = new Dictionary<IFieldReference, AddressWrapper>(frc);
                }
            }
            else
            {
                IDictionary<IFieldReference, AddressWrapper> innerDict = new Dictionary<IFieldReference, AddressWrapper>(frc);
                InstFldRefToAddrWrapperMap.Add(inst, innerDict);   
            }
            InstructionWrapper instW = getInstW(inst, meth);
            FieldRefWrapper fldW = getFieldRefW(fldRef);
            AddressWrapper addW = new AddressWrapper(instW, fldW);
            InstFldRefToAddrWrapperMap[inst][fldRef] = addW;
            return addW;
        }

        public static AddressWrapper getAddrW(Instruction newArrInst, IMethodDefinition meth)
        {
            if (ArrayToAddrWrapperMap.ContainsKey(newArrInst))
            {
                return ArrayToAddrWrapperMap[newArrInst];
            }
            else
            {
                InstructionWrapper instW = getInstW(newArrInst, meth);
                FieldRefWrapper fldW = ProgramDoms.domF.GetVal(0);
                AddressWrapper addW = new AddressWrapper(instW, fldW);
                ArrayToAddrWrapperMap[newArrInst] = addW;
                return addW;
            }
        }

        public static AddressWrapper getAddrW(IFieldReference fldRef)
        {
            if (FieldRefToAddrWrapperMap.ContainsKey(fldRef))
            {
                return FieldRefToAddrWrapperMap[fldRef];
            }
            else
            {
                FieldRefWrapper fldW = getFieldRefW(fldRef);
                AddressWrapper addW = new AddressWrapper(fldW);
                FieldRefToAddrWrapperMap[fldRef] = addW;
                return addW;
            }
        }

        public static AddressWrapper getAddrW(IVariable var)
        {
            if (VarToAddrWrapperMap.ContainsKey(var))
            {
                return VarToAddrWrapperMap[var];
            }
            else
            {
                VariableWrapper varW = getVarW(var);
                AddressWrapper addW = new AddressWrapper(varW);
                VarToAddrWrapperMap[var] = addW;
                return addW;
            }
        }

        public static HeapAccWrapper getHeapAccW(IVariable var)
        {
            if (VarToHeapAccWrapperMap.ContainsKey(var))
            {
                return VarToHeapAccWrapperMap[var];
            }
            else
            {
                VariableWrapper varW = getVarW(var);
                HeapAccWrapper hpW = new HeapAccWrapper(varW);
                VarToHeapAccWrapperMap[var] = hpW;
                return hpW;
            }
        }

        public static HeapAccWrapper getHeapAccW(Instruction inst, IMethodDefinition meth)
        {
            if (InstToHeapAccWrapperMap.ContainsKey(inst))
            {
                return InstToHeapAccWrapperMap[inst];
            }
            else
            {
                InstructionWrapper instW = getInstW(inst, meth);
                HeapAccWrapper hpW = new HeapAccWrapper(instW);
                InstToHeapAccWrapperMap[inst] = hpW;
                return hpW;
            }
        }

        public static ExHandlerWrapper getExHandlerW(Instruction inst, IMethodDefinition meth)
        {
            if (InstToExHandlerWrapperMap.ContainsKey(inst))
            {
                return InstToExHandlerWrapperMap[inst];
            }
            else
            {
                InstructionWrapper instW = getInstW(inst, meth);
                ExHandlerWrapper ehW = new ExHandlerWrapper(instW);
                InstToExHandlerWrapperMap[inst] = ehW;
                return ehW;
            }
        }
    }
}
