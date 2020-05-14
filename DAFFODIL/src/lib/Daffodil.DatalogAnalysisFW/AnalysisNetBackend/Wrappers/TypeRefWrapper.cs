// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.Cci;
using Daffodil.DatalogAnalysisFW.AnalysisNetConsole;
using Daffodil.DatalogAnalysisFW.ProgramFacts;


namespace Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers
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
