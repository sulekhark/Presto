using Daffodil.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Daffodil.ExceptionFlowAnalysis.ProgramFacts.Doms
{
    public class DomN : Dom<IntWrapper>
    {
        public DomN() : base("N")
        {
            for (int i = 0; i < 32767; i++)
            {
                base.Add(new IntWrapper(i));
            }
        }
    }
}

