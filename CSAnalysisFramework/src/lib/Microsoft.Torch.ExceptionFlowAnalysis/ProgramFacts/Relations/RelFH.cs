﻿using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelFH : Rel
    {
        public RelFH() : base(2, "FH")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domF.GetName();
            domNames[1] = ProgramDoms.domH.GetName();
        }

        public bool Add(FieldRefWrapper fldRefW, InstructionWrapper allocW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domF.IndexOf(fldRefW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domH.IndexOf(allocW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
