using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Values;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole;
using Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts;


namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public class VariableWrapper : IWrapper
    {
        readonly string varName;
        readonly string className;
        readonly string methodName;

        public VariableWrapper(IVariable var)
        {
            varName = var.Method.Name.Value + "::" + var.ToString();
            methodName = var.Method.FullName();
            className = var.Method.ContainingType.FullName();
        }

        public override string ToString()
        {
            return varName;
        }

        public string GetDesc()
        {
            string s = "CLASS:" + className + " METHOD:" + methodName;
            return s;
        }
    }
}
