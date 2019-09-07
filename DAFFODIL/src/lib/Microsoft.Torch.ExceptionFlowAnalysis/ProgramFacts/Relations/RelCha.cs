using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelCha : Rel
    {
        public RelCha() : base(3, "cha")
        {
            domNames = new string[3];
            domNames[0] = ProgramDoms.domM.GetName();
            domNames[1] = ProgramDoms.domT.GetName();
            domNames[2] = ProgramDoms.domM.GetName();
        }

        public bool Add(MethodRefWrapper mRefW1, TypeRefWrapper typRefW, MethodRefWrapper mRefW2)
        {
            int[] iarr = new int[3];

            iarr[0] = ProgramDoms.domM.IndexOf(mRefW1);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domT.IndexOf(typRefW);
            if (iarr[1] == -1) return false;
            iarr[2] = ProgramDoms.domM.IndexOf(mRefW2);
            if (iarr[2] == -1) return false;
            return base.Add(iarr);
        }
    }
}
