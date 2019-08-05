using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public class TypeRefWrapper
    {
        ITypeReference type;

        public TypeRefWrapper(ITypeReference type)
        {
            this.type = type;
        }

        public override string ToString()
        {
            return type.FullName();
        }
    }
}
