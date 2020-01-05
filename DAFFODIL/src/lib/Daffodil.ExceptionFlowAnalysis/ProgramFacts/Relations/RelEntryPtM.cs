﻿using Daffodil.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Daffodil.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelEntryPtM : Rel
    {
        public RelEntryPtM() : base(1, "entryPtM")
        {
            domNames = new string[1];
            domNames[0] = ProgramDoms.domM.GetName();
        }

        public bool Add(MethodRefWrapper methRefW)
        {
            int[] iarr = new int[1];

            iarr[0] = ProgramDoms.domM.IndexOf(methRefW);
            if (iarr[0] == -1) return false;
            return base.Add(iarr);
        }
    }
}
