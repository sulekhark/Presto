using Daffodil.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Daffodil.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelThrowMH : Rel
    {
        public RelThrowMH() : base(2, "ThrowMH")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domM.GetName();
            domNames[1] = ProgramDoms.domH.GetName();
        }

        public bool Add(MethodRefWrapper methW, HeapElemWrapper allocW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domM.IndexOf(methW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domH.IndexOf(allocW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
