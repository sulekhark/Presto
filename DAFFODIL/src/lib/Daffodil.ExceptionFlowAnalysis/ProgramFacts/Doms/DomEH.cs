
using Daffodil.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;


namespace Daffodil.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomEH : Dom<ExHandlerWrapper>
    {
        public DomEH() : base("EH") { }
    }
}
