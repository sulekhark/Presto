using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelStructV : Rel
    {
        public RelStructV() : base(1, "structV")
        {
            domNames = new string[1];
            domNames[0] = ProgramDoms.domV.GetName();
        }

        public bool Add(VariableWrapper varW)
        {
            int[] iarr = new int[1];

            iarr[0] = ProgramDoms.domV.IndexOf(varW);
            if (iarr[0] == -1) return false;
            return base.Add(iarr);
        }
    }
}
