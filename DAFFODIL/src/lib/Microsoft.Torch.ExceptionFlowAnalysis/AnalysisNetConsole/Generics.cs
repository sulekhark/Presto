using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.Cci.Immutable;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole
{
    public static class Generics
    {
        private static IInternFactory internFactory;

        public static void SetupInternFactory(IInternFactory ifactory)
        {
            internFactory = ifactory;
        }

        public static IMethodDefinition GetTemplate(IMethodDefinition m)
        {
            return (m as IGenericMethodInstance).GenericMethod.ResolvedMethod;
        }

        public static IMethodDefinition RecordInfo(IMethodDefinition templateMeth, IMethodDefinition instMeth, bool createIfReqd)
        {
            IDictionary<string, IMethodDefinition> instMeths;
            if (MetadataVisitor.genericMethodMap.ContainsKey(templateMeth))
            {
                instMeths = MetadataVisitor.genericMethodMap[templateMeth];
            }
            else
            {
                instMeths = new Dictionary<string, IMethodDefinition>();
                MetadataVisitor.genericMethodMap.Add(templateMeth, instMeths);
            }
            IMethodDefinition retMeth;
            if (!createIfReqd)
            {
                string argStr = GetGenericArgStr((instMeth as IGenericMethodInstance).GenericArguments);
                if (!instMeths.ContainsKey(argStr)) instMeths[argStr] = instMeth;
                retMeth = instMeths[argStr];
            }
            else
            {
                retMeth = GetInstantiatedMeth(templateMeth, instMeth);
            }
            return retMeth;
        }

        public static string GetGenericArgStr(IEnumerable<ITypeReference> argList)
        {
            string argStr = "";
            foreach (ITypeReference garg in argList)
            {
                argStr = argStr + garg.FullName() + ',';
            }
            argStr.TrimEnd(',');
            return argStr;
        }

        public static IMethodDefinition GetInstantiatedMeth(IMethodDefinition templateMeth, IMethodDefinition instMeth)
        {
            IGenericMethodInstance genericM = instMeth as IGenericMethodInstance;
            IEnumerable<ITypeReference> genericArgs = genericM.GenericArguments;
            IList<ITypeReference> stubbedArgList = new List<ITypeReference>();
            foreach (ITypeReference garg in genericArgs)
            {
                ITypeDefinition addedType = Stubber.CheckAndAdd(garg.ResolvedType);
                stubbedArgList.Add(addedType);
            }
            string argStr = GetGenericArgStr(stubbedArgList);
            IDictionary<string, IMethodDefinition> instMap;
            if (MetadataVisitor.genericMethodMap.ContainsKey(templateMeth))
            {
                instMap = MetadataVisitor.genericMethodMap[templateMeth];
            }
            else
            {
                instMap = new Dictionary<string, IMethodDefinition>();
                MetadataVisitor.genericMethodMap[templateMeth] = instMap;
            }
            if (instMap.ContainsKey(argStr))
            {
                return instMap[argStr];
            }
            else
            {
                GenericMethodInstanceReference newInstMethRef = new GenericMethodInstanceReference(templateMeth,
                                                                    genericArgs, internFactory);
                IMethodDefinition newInstMeth = newInstMethRef.ResolvedMethod;
                instMap[argStr] = newInstMeth;
                return newInstMeth;
            }
        }
    }
}
