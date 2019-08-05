using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Values;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public class VariableWrapper
    {
        string varName;

        public VariableWrapper(IVariable var)
        {
            this.varName = var.Method.FullName() + "::" + var.ToString();
        }

        public override string ToString()
        {
            return varName;
        }
    }
}
