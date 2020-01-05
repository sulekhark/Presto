using Daffodil.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Daffodil.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelHFHO : Rel
    {
        public RelHFHO() : base(3, "HFHO")
        {
            domNames = new string[3];
            domNames[0] = ProgramDoms.domH.GetName();
            domNames[1] = ProgramDoms.domF.GetName();
            domNames[2] = ProgramDoms.domH.GetName();
        }

        public bool Add(HeapElemWrapper allocW1, FieldRefWrapper fldRefW, HeapElemWrapper allocW2)
        {
            int[] iarr = new int[3];

            iarr[0] = ProgramDoms.domH.IndexOf(allocW1);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domF.IndexOf(fldRefW);
            if (iarr[1] == -1) return false;
            iarr[2] = ProgramDoms.domH.IndexOf(allocW2);
            if (iarr[2] == -1) return false;
            return base.Add(iarr);
        }
    }
}
