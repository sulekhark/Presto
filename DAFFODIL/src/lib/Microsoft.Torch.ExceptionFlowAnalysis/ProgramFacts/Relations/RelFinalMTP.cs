﻿using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;


namespace Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelFinalMTP : Rel
    {
        public RelFinalMTP() : base(3, "FinalMTP")
        {
            domNames = new string[3];
            domNames[0] = ProgramDoms.domM.GetName();
            domNames[1] = ProgramDoms.domT.GetName();
            domNames[2] = ProgramDoms.domP.GetName();
        }

        public bool Add(MethodRefWrapper methW, TypeRefWrapper typeRefW, InstructionWrapper instW)
        {
            int[] iarr = new int[3];

            iarr[0] = ProgramDoms.domM.IndexOf(methW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domT.IndexOf(typeRefW);
            if (iarr[1] == -1) return false;
            iarr[2] = ProgramDoms.domP.IndexOf(instW);
            if (iarr[2] == -1) return false;
            return base.Add(iarr);
        }
    }
}