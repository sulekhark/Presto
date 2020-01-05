using Daffodil.ExceptionFlowAnalysis.ProgramFacts;

namespace Daffodil.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public class IntWrapper : IWrapper
    {
        int val;

        public IntWrapper(int n)
        {
            val = n;
        }

        public override string ToString()
        {
            return val.ToString();
        }

        public string GetDesc()
        {
            return "";
        }
    }
}
