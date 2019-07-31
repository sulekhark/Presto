
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;


namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomI : Dom<InstructionWrapper>
    {
        public DomI() : base("I") { }

    }
}
