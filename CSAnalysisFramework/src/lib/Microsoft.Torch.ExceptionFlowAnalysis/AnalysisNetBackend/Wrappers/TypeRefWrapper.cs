using Microsoft.Cci;

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
            return type.ToString();
        }
    }
}
