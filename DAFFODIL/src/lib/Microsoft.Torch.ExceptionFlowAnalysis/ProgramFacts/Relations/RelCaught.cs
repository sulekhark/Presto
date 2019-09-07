using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelCaught : Rel
    {
        public RelCaught() : base(2, "Caught")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domT.GetName();
            domNames[1] = ProgramDoms.domH.GetName();
        }

        public bool Add(TypeRefWrapper typeRefW, HeapAccWrapper allocW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domT.IndexOf(typeRefW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domH.IndexOf(allocW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
