using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Values;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public class VariableWrapper
    {
        IVariable var;

        public VariableWrapper(IVariable var)
        {
            this.var = var;
        }

        public override string ToString()
        {
            string varStr = var.Method.Name + "::" + var.ToString();
            return varStr;
        }
    }
}
