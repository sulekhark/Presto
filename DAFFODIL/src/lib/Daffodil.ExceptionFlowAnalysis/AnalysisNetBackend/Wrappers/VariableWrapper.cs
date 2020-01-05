using Daffodil.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Values;
using Daffodil.ExceptionFlowAnalysis.AnalysisNetConsole;
using Daffodil.ExceptionFlowAnalysis.ProgramFacts;


namespace Daffodil.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
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
