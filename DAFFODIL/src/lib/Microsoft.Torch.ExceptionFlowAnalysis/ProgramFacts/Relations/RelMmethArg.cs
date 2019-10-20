
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelMmethArg : Rel
    {
        public RelMmethArg() : base(3, "MmethArg")
        {
            domNames = new string[3];
            domNames[0] = ProgramDoms.domM.GetName();
            domNames[1] = ProgramDoms.domZ.GetName();
            domNames[2] = ProgramDoms.domV.GetName();
        }

        public bool Add(MethodRefWrapper mRefW, int argNum, VariableWrapper argW)
        {
            int[] iarr = new int[3];

            iarr[0] = ProgramDoms.domM.IndexOf(mRefW);
            if (iarr[0] == -1) return false;
            iarr[1] = argNum;
            iarr[2] = ProgramDoms.domV.IndexOf(argW);
            if (iarr[2] == -1) return false;
            return base.Add(iarr);
        }
    }
}