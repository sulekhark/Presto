using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;


namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelVirtIM : Rel
    {
        public RelVirtIM() : base(2, "VirtIM")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domI.GetName();
            domNames[1] = ProgramDoms.domM.GetName();
        }

        public bool Add(InstructionWrapper invkW, MethodRefWrapper mCallee)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domI.IndexOf(invkW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domM.IndexOf(mCallee);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
