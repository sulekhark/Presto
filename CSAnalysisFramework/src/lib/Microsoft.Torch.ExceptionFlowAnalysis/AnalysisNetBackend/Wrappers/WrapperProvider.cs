
using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Instructions;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Values;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Model;

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
        }

        public static MethodRefWrapper getMethodRefW (IMethodReference methRef)
        {
            if (MethRefToWrapperMap.ContainsKey(methRef))
            {
                // System.Console.WriteLine("SRK_DBG1: Returning existing");
                MethodRefWrapper mRefW = MethRefToWrapperMap[methRef];
                // System.Console.WriteLine("SRK_DBG: HELLO: {0}", mRefW.ToString());
                return mRefW;
            }
            else
            {
                // System.Console.WriteLine("SRK_DBG1: Creating new");
                MethodRefWrapper mRefW = new MethodRefWrapper(methRef);
                MethRefToWrapperMap.Add(methRef, mRefW);
                // System.Console.WriteLine("SRK_DBG: HELLO: {0}", mRefW.ToString());
                return mRefW;
            }
        }

        public static MethodRefWrapper getMethodRefW(IMethodReference methRef, MethodBody methBody)
        {
            if (MethRefToWrapperMap.ContainsKey(methRef))
            {
                MethodRefWrapper mRefW = MethRefToWrapperMap[methRef];
                // System.Console.WriteLine("SRK_DBG2: Calling update");
                mRefW.Update(methBody);
                // System.Console.WriteLine("SRK_DBG: HELLO: {0}", mRefW.ToString());
                return mRefW;
            }
            else
            {
                // System.Console.WriteLine("SRK_DBG2: Creating new");
                MethodRefWrapper mRefW = new MethodRefWrapper(methRef, methBody);
                MethRefToWrapperMap.Add(methRef, mRefW);
                // System.Console.WriteLine("SRK_DBG: HELLO: {0}", mRefW.ToString());
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

        public static InstructionWrapper getInstW(Instruction inst)
        {
            if (InstToWrapperMap.ContainsKey(inst))
            {
                return InstToWrapperMap[inst];
            }
            else
            {
                InstructionWrapper instW = new InstructionWrapper(inst);
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

        public static AddressWrapper getAddrW(Instruction inst, IFieldReference fldRef)
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
            InstructionWrapper instW = getInstW(inst);
            FieldRefWrapper fldW = getFieldRefW(fldRef);
            AddressWrapper addW = new AddressWrapper(instW, fldW);
            InstFldRefToAddrWrapperMap[inst][fldRef] = addW;
            return addW;
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
    }
}
