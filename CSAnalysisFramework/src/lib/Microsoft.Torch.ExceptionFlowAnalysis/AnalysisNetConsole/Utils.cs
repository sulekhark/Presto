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
        public static bool ImplementsInterface(ITypeDefinition tgt, ITypeDefinition qItf)
        {
            if (qItf == null || tgt == null) return false;
            foreach (ITypeReference itf in tgt.Interfaces)
            {
                if (itf != null && qItf.Equals(itf.ResolvedType)) return true;
            }
            foreach (ITypeReference itf in tgt.Interfaces)
            {
                if (ImplementsInterface(itf.ResolvedType, qItf)) return true;
            }
            return false;
        }

        public static bool ExtendsClass(ITypeDefinition derivedCl, ITypeDefinition baseCl)
        {
            if (derivedCl.Equals(baseCl)) return true;
            if (derivedCl == null || baseCl == null) return false;
            foreach (ITypeReference bcl in derivedCl.BaseClasses)
            {
                if (bcl != null && baseCl.Equals(bcl.ResolvedType)) return true;
            }
            foreach (ITypeReference bcl in derivedCl.BaseClasses)
            {
                if (ExtendsClass(bcl.ResolvedType, baseCl)) return true;
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

        public static bool SignMatch(IMethodReference m1, IMethodReference m2)
        {
            if (m1 == null || m2 == null) return false;
            string m1Sign = m1.FullName();
            string m2Sign = m2.FullName();
            return (m1Sign == m2Sign);
        }

        public static IMethodDefinition GetSignMatchMethod(ITypeDefinition ty, IMethodDefinition meth)
        {
            foreach (IMethodDefinition tyMeth in ty.Methods)
            {
                if (SignMatch(meth, tyMeth)) return tyMeth;
            }
            return null;
        }

        public static IMethodDefinition GetMethodByName(ITypeDefinition ty, string methName)
        {
            foreach (IMethodDefinition tyMeth in ty.Methods)
            {
                if (tyMeth.FullName() == methName) return tyMeth;
            }
            return null;
        }

        public static bool IsMainMethod (IMethodDefinition meth)
        {
            if (meth.GetName() == "Main" && meth.IsStatic) return true;     
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
                    Instruction firstInst = node.Instructions.First();
                    if (firstInst is PhiInstruction)
                    {
                        firstInst = node.Instructions.ElementAt(1);
                    }
                    string lbl = firstInst.Label;
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
