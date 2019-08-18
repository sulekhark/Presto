
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;


namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomEH : Dom<ExHandlerWrapper>
    {
        public DomEH() : base("EH") { }
    }
}
