
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;


namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomH : Dom<HeapElemWrapper>
    {
        public DomH() : base("H") { }

    }
}
