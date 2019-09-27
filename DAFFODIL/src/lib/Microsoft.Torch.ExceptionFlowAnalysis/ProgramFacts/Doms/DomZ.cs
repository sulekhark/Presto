using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomZ : Dom<IntWrapper>
    {
        public DomZ() : base("Z")
        {
            for (int i = 0; i < 64; i++)
            {
                base.Add(new IntWrapper(i));
            }
        }
    }
}
