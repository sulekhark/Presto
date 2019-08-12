using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Model;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Instructions;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole
{
    public static class Utils
    {
        static TypeDefinitionComparer tdc;

        static Utils()
        {
            tdc = new TypeDefinitionComparer();
        }

        public static bool ImplementsInterface(ITypeDefinition tgt, ITypeDefinition queryItf)
        {
            if (queryItf == null || tgt == null) return false;
            
            foreach (ITypeReference itf in tgt.Interfaces)
            {
                ITypeDefinition analItf = Stubber.GetTypeToAnalyze(itf.ResolvedType);
                if (analItf != null && tdc.Equals(queryItf, analItf.ResolvedType)) return true;
            }
            foreach (ITypeReference itf in tgt.Interfaces)
            {
                ITypeReference analItf = Stubber.GetTypeToAnalyze(itf.ResolvedType);
                if (analItf != null && ImplementsInterface(analItf.ResolvedType, queryItf)) return true;
            }
            return false;
        }

        public static bool ExtendsClass(ITypeDefinition derivedCl, ITypeDefinition baseCl)
        {
            if (derivedCl == null || baseCl == null) return false;
            ITypeDefinition analBaseCl = Stubber.GetTypeToAnalyze(baseCl);
            if (analBaseCl != null && tdc.Equals(derivedCl, analBaseCl)) return true;

            foreach (ITypeReference bcl in derivedCl.BaseClasses)
            {
                ITypeDefinition analBcl = Stubber.GetTypeToAnalyze(bcl.ResolvedType);
                if (analBcl != null && tdc.Equals(baseCl, analBcl)) return true;
            }
            foreach (ITypeReference bcl in derivedCl.BaseClasses)
            {
                ITypeDefinition analBcl = Stubber.GetTypeToAnalyze(bcl.ResolvedType);
                if (ExtendsClass(analBcl, baseCl)) return true;
            }
            return false;
        }

        public static string FullName(this ITypeReference tref)
        {
            return TypeHelper.GetTypeName(tref, NameFormattingOptions.Signature | NameFormattingOptions.TypeParameters);
        }
        public static string GetName(this ITypeReference tref)
        {
            if (tref is INamedTypeReference)
                return (tref as INamedTypeReference).Name.Value;

            return TypeHelper.GetTypeName(tref, NameFormattingOptions.OmitContainingType | NameFormattingOptions.OmitContainingNamespace | NameFormattingOptions.SmartTypeName);
        }

        public static string FullName(this IMethodReference mref)
        {
            return MemberHelper.GetMethodSignature(mref, NameFormattingOptions.Signature | NameFormattingOptions.ParameterName | NameFormattingOptions.TypeParameters);
        }

        public static string GetName(this IMethodReference mref)
        {
            return MemberHelper.GetMethodSignature(mref, NameFormattingOptions.Signature | NameFormattingOptions.TypeParameters);
        }

        // Full Name but without containing name space should match: <method_name><<generic_param_if_present>>(<param_list>)
        // This is for finding the same method definition but in a stub namespace.
        public static bool StubMatch(IMethodReference m1, IMethodReference m2)
        {
            if (m1 == null || m2 == null) return false;
            string m1Sign = MemberHelper.GetMethodSignature(m1, NameFormattingOptions.OmitContainingType | NameFormattingOptions.Signature | NameFormattingOptions.ParameterName | NameFormattingOptions.TypeParameters);
            string m2Sign = MemberHelper.GetMethodSignature(m2, NameFormattingOptions.OmitContainingType | NameFormattingOptions.Signature | NameFormattingOptions.ParameterName | NameFormattingOptions.TypeParameters);
            return (m1Sign == m2Sign);
        }

        // method name, formal parameters and number of generic parameters must match
        public static bool GenericStubMatch(IMethodReference m1, IMethodReference m2)
        {
            IGenericMethodInstance gm1 = m1 as IGenericMethodInstance;
            IGenericMethodInstance gm2 = m2 as IGenericMethodInstance;
            if (gm1 == null || gm2 == null) return false;
            
            string m1Sign = MemberHelper.GetMethodSignature(gm1, NameFormattingOptions.OmitContainingType | NameFormattingOptions.Signature | NameFormattingOptions.ParameterName);
            string m2Sign = MemberHelper.GetMethodSignature(gm2, NameFormattingOptions.OmitContainingType | NameFormattingOptions.Signature | NameFormattingOptions.ParameterName);
           
            int num1 = gm1.GenericParameterCount;
            int num2 = gm2.GenericParameterCount;
            return (m1Sign == m2Sign) && (num1 == num2);
        }

        // Full Name should match: <containing_name_space>.<method_name><<generic_param_if_present>>(<param_list>)
        public static IMethodDefinition GetSignMatchMethod(ITypeDefinition ty, IMethodDefinition meth)
        {
            if (meth is IGenericMethodInstance)
            {
                foreach (IMethodDefinition tyMeth in ty.Methods)
                {
                    if (MemberHelper.GenericMethodSignaturesAreEqual(meth, tyMeth)) return tyMeth;
                }
            }
            else
            {
                foreach (IMethodDefinition tyMeth in ty.Methods)
                {
                    if (MemberHelper.SignaturesAreEqual(meth, tyMeth)) return tyMeth;
                }
            }
            return null;
        }

        // Full Name but without containing name space should match: <method_name><<generic_param_if_present>>(<param_list>)
        // This is for finding the same method definition but in a stub namespace.
        public static IMethodDefinition GetStubMatchMethod(ITypeDefinition ty, IMethodDefinition meth)
        {
            foreach (IMethodDefinition tyMeth in ty.Methods)
            {
                if (StubMatch(meth, tyMeth)) return tyMeth;
            }
            return null;
        }

        public static IMethodDefinition GetMethodByName(ITypeDefinition ty, string methName)
        {
            foreach (IMethodDefinition tyMeth in ty.Methods)
            {
                if (tyMeth.Name.Value == methName) return tyMeth;
            }
            return null;
        }

        public static IMethodDefinition GetMethodByFullName(ITypeDefinition ty, string methName)
        {
            foreach (IMethodDefinition tyMeth in ty.Methods)
            {
                if (tyMeth.FullName() == methName) return tyMeth;
            }
            return null;
        }

        public static bool IsMainMethod (IMethodDefinition meth)
        {
            if (meth.Name.Value == "Main" && meth.IsStatic) return true;     
            return false;
        }

        public static IMethodDefinition GetStaticConstructor(ITypeDefinition ty)
        {
            foreach (IMethodDefinition meth in ty.Methods)
            {
                if (meth.IsStaticConstructor) return meth;
            }
            return null;
        }

        public static IList<CFGNode> getProgramFlowOrder(ControlFlowGraph cfg)
        {
            IList<CFGNode> cfgNodeList = new List<CFGNode>();
            IDictionary<int, CFGNode> addrToNodeMap = new Dictionary<int, CFGNode>();

            foreach (var node in cfg.Nodes)
            {
                if (node.Instructions.Count > 0)
                {
                    Instruction firstNonPhiInst = null;
                    foreach (Instruction inst in node.Instructions)
                    {
                        if (inst is PhiInstruction) continue;
                        firstNonPhiInst = inst;
                        break;
                    }
                    string lbl = firstNonPhiInst.Label;
                    string pcStr = lbl.Substring(2);
                    int pcVal = Int32.Parse(pcStr, System.Globalization.NumberStyles.HexNumber);
                    addrToNodeMap.Add(pcVal, node);
                }
            }
            int[] addrs = addrToNodeMap.Keys.ToArray();
            Array.Sort(addrs);
            for (int i = 0; i < addrs.Count(); i++)
            {
                cfgNodeList.Add(addrToNodeMap[addrs[i]]);
            }
            return cfgNodeList;
        }
    }
}
