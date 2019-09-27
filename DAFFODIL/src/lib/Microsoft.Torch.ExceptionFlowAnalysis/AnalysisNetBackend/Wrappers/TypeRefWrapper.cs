﻿using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole;
using Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts;


namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public class TypeRefWrapper : IWrapper
    {
        readonly ITypeReference type;
        readonly string moduleName;

        public TypeRefWrapper(ITypeReference type)
        {
            this.type = type;
            IModule mod = TypeHelper.GetDefiningUnit(type.ResolvedType) as IModule;
            moduleName = (mod == null) ? "UNK" : mod.Name.Value;
        }

        public override string ToString()
        {
            return type.FullName();
        }

        public string GetDesc()
        {
            string s = "MODULE:" + moduleName;
            return s;
        }
    }
}
