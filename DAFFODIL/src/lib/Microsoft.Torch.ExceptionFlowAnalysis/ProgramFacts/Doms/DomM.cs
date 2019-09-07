
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomM : Dom<MethodRefWrapper>
    {
        public DomM() : base("M") { }

    }
}
