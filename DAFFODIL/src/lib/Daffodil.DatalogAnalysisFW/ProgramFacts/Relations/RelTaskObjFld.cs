using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelTaskObjFld : Rel
    {
        public RelTaskObjFld() : base(2, "TaskObjFld")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domH.GetName();
            domNames[1] = ProgramDoms.domF.GetName();
        }

        public bool Add(HeapElemWrapper allocW, FieldRefWrapper fldRefW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domH.IndexOf(allocW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domF.IndexOf(fldRefW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
