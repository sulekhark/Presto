using Daffodil.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Daffodil.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomZ : Dom<IntWrapper>
    {
        public DomZ() : base("Z")
        {
            for (int i = 0; i < 32; i++)
            {
                base.Add(new IntWrapper(i));
            }
        }
    }
}
