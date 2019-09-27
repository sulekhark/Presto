using Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
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
