using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelFXO : Rel
    {
        public RelFXO() : base(2, "FXO")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domF.GetName();
            domNames[1] = ProgramDoms.domX.GetName();
        }

        public bool Add(FieldRefWrapper fldRefW, AddressWrapper addrW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domF.IndexOf(fldRefW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domX.IndexOf(addrW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
