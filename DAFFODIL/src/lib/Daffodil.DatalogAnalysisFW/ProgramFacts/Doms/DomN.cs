using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Doms
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

