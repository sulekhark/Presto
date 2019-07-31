
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

        static WrapperProvider()
        {
            MethRefToWrapperMap = new Dictionary<IMethodReference, MethodRefWrapper>(MethodReferenceDefinitionComparer.Default);
            TypeRefToWrapperMap = new Dictionary<ITypeReference, TypeRefWrapper>(new TypeDefinitionComparer());
            FieldRefToWrapperMap = new Dictionary<IFieldReference, FieldRefWrapper>(new FieldReferenceComparer());
            InstToWrapperMap = new Dictionary<Instruction, InstructionWrapper>(new InstructionComparer());
            VarToWrapperMap = new Dictionary<IVariable, VariableWrapper>(new VariableComparer());
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
    }
}
