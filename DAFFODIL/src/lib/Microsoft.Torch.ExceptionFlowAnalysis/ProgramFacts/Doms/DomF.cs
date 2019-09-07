using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomF : Dom<FieldRefWrapper>
    {
        public DomF() : base("F") { }

    }
}
