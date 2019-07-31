
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;


namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomC : Dom<InstructionWrapper>
    {
        public DomC() : base("C") { }

    }
}
