﻿using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelThrowPV : Rel
    {
        public RelThrowPV() : base(2, "ThrowPV")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domP.GetName();
            domNames[1] = ProgramDoms.domV.GetName();
        }

        public bool Add(InstructionWrapper instW, VariableWrapper varW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domP.IndexOf(instW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domV.IndexOf(varW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
