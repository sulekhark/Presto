
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;


namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomH : Dom<HeapAccWrapper>
    {
        public DomH() : base("H") { }

    }
}
