using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.Cci.Immutable;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole
{
    public static class Generics
    {
        private static IInternFactory internFactory;
        private static readonly IDictionary<uint, IDictionary<string, IMethodDefinition>> stubTemplateInstMap;

        static Generics()
        {
            stubTemplateInstMap = new Dictionary<uint, IDictionary<string, IMethodDefinition>>();
        }

        public static void SetupInternFactory(IInternFactory ifactory)
        {
            internFactory = ifactory;
        }

        public static IMethodDefinition GetTemplate(IMethodDefinition m)
        {
            return (m as IGenericMethodInstance).GenericMethod.ResolvedMethod;
        }

        public static IMethodDefinition RecordInfo(IMethodDefinition templateMeth, IMethodDefinition instMeth, bool isStubbed)
        {
            ISet<IMethodDefinition> instMeths;
            if (MetadataVisitor.genericMethodMap.ContainsKey(templateMeth))
            {
                instMeths = MetadataVisitor.genericMethodMap[templateMeth];
            }
            else
            {
                instMeths = new HashSet<IMethodDefinition>();
                MetadataVisitor.genericMethodMap.Add(templateMeth, instMeths);
            }
            IMethodDefinition retMeth;
            if (!isStubbed)
            {
                instMeths.Add(instMeth);
                retMeth = instMeth;
            }
            else
            {
                retMeth = GetInstantiatedMeth(templateMeth, instMeth);
                instMeths.Add(retMeth);
            }
            return retMeth;
        }

        public static IMethodDefinition GetInstantiatedMeth(IMethodDefinition templateMeth, IMethodDefinition instMeth)
        {
            IGenericMethodInstance genericM = instMeth as IGenericMethodInstance;
            IEnumerable<ITypeReference> genericArgs = genericM.GenericArguments;
            string argStr = genericArgs.ToString();
            uint templateKey = templateMeth.InternedKey;
            IDictionary<string, IMethodDefinition> instMap = null;
            if (stubTemplateInstMap.ContainsKey(templateKey))
            {
                instMap = stubTemplateInstMap[templateKey];
            }
            else
            {
                instMap = new Dictionary<string, IMethodDefinition>();
                stubTemplateInstMap.Add(templateKey, instMap);
            }
            if (instMap.ContainsKey(argStr))
            {
                return instMap[argStr];
            }
            else
            {
                GenericMethodInstance newInstMeth = new GenericMethodInstance(templateMeth, genericArgs, internFactory);
                instMap[argStr] = newInstMeth;
                return newInstMeth;
            }
        }
    }
}
