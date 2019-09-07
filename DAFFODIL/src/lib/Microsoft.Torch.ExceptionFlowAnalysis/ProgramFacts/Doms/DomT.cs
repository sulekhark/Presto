
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomT : Dom<TypeRefWrapper>
    {
        public DomT() : base("T") { }

    }
}
