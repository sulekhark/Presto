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

            return TypeHelper.GetTypeName(tref, NameFormattingOptions.OmitContainingType | 
                NameFormattingOptions.OmitContainingNamespace | NameFormattingOptions.SmartTypeName);
        }

        public static string FullName(this IMethodReference mref)
        {
            return MemberHelper.GetMethodSignature(mref, NameFormattingOptions.Signature | NameFormattingOptions.ParameterName |
                NameFormattingOptions.TypeParameters | NameFormattingOptions.PreserveSpecialNames);
        }

        public static string GetName(this IMethodReference mref)
        {
            return MemberHelper.GetMethodSignature(mref, NameFormattingOptions.Signature | NameFormattingOptions.TypeParameters |
                NameFormattingOptions.PreserveSpecialNames);
        }

        public static bool NameMatch(IMethodReference m1, IMethodReference m2)
        {
            string name1 = m1.Name.Value;
            string name2 = m2.Name.Value;
            string extName1 = "." + name1;
            string extName2 = "." + name2;
            return (name1 == name2 || name1.EndsWith(extName2) || name2.EndsWith(extName1));
        }

        public static bool MethodSignMatch(IMethodReference m1, IMethodReference m2)
        {
            bool signEq = false;
            if (!m1.IsGeneric && !m2.IsGeneric) signEq = MemberHelper.SignaturesAreEqual(m1, m2);
            else if (m1.IsGeneric && m2.IsGeneric) signEq = MemberHelper.GenericMethodSignaturesAreEqual(m1, m2);
            if (signEq && !NameMatch(m1, m2)) signEq = false;
            return signEq;
        }

        public static IMethodDefinition GetMethodSignMatch(ITypeDefinition ty, IMethodDefinition meth)
        {
            foreach (IMethodDefinition tyMeth in ty.Methods)
            {
                if (MethodSignMatch(meth, tyMeth)) return tyMeth;
            }
            return null;
        }

        public static bool GenericInstMethodSignMatch(IMethodReference m1, IMethodReference m2)
        {
            bool signEq = MemberHelper.GenericMethodSignaturesAreEqual(m1, m2);
            if (signEq && !NameMatch(m1, m2)) signEq = false;
            if (signEq)
            {
                IGenericMethodInstance gm1 = m1 as IGenericMethodInstance;
                IGenericMethodInstance gm2 = m2 as IGenericMethodInstance;
                IList<ITypeReference> genArgs1 = gm1.GenericArguments.ToList();
                IList<ITypeReference> genArgs2 = gm2.GenericArguments.ToList();
                if (genArgs1.Count() == genArgs2.Count())
                {
                    for (int i = 0; i < genArgs1.Count(); i++)
                    {
                        if (genArgs1[i].InternedKey != genArgs2[i].InternedKey)
                        {
                            signEq = false;
                            break;
                        }
                    }
                }
                else signEq = false;
            }
            return signEq;
        }

        public static IMethodDefinition GetGenericInstMethodSignMatch(ISet<IMethodDefinition>candidateInsts,
                                                                      IMethodDefinition meth)
        {
            foreach (IMethodDefinition cMeth in candidateInsts)
            {
                if (GenericInstMethodSignMatch(meth, cMeth)) return cMeth;
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

        // If P is some property of a class, FullName (which uses CCI's GetMethodSignature) returns P.get whereas internally
        // the name of the method is get_P (ILSpy also shows the name as get_P). 
        // Should be aware of this while using string compare for method names.
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
