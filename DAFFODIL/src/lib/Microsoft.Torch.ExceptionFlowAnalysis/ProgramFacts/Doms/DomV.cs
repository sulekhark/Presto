
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomV : Dom<VariableWrapper>
    {
        public DomV() : base("V") { }

    }
}
