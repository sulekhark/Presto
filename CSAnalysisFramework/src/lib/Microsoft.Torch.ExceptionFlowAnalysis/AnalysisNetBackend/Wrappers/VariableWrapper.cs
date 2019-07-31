using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Values;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public class VariableWrapper
    {
        readonly string varStr;

        public VariableWrapper(IVariable var)
        {
            varStr = var.Method.Name + "::" + var.ToString();
        }

        public override string ToString()
        {
            return varStr;
        }
    }
}
