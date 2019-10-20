﻿using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelRootM : Rel
    {
        public RelRootM() : base(1, "rootM")
        {
            domNames = new string[1];
            domNames[0] = ProgramDoms.domM.GetName();
        }

        public bool Add(MethodRefWrapper methW)
        {
            int[] iarr = new int[1];

            iarr[0] = ProgramDoms.domM.IndexOf(methW);
            if (iarr[0] == -1) return false;
            return base.Add(iarr);
        }
    }
}