
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;


namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomH : Dom<InstructionWrapper>
    {
        public DomH() : base("H") { }

    }
}
