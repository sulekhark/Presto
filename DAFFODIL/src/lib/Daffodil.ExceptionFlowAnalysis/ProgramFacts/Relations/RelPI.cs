using Daffodil.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Daffodil.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelPI : Rel
    {
        public RelPI() : base(2, "PI")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domP.GetName();
            domNames[1] = ProgramDoms.domI.GetName();
        }

        public bool Add(InstructionWrapper pgmPtW, InstructionWrapper invkW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domP.IndexOf(pgmPtW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domI.IndexOf(invkW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
