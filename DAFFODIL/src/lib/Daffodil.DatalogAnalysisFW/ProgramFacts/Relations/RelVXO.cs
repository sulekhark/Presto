using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelVXO : Rel
    {
        public RelVXO() : base(2, "VXO")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domV.GetName();
            domNames[1] = ProgramDoms.domX.GetName();
        }

        public bool Add(VariableWrapper varW, AddressWrapper addrW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domV.IndexOf(varW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domX.IndexOf(addrW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
