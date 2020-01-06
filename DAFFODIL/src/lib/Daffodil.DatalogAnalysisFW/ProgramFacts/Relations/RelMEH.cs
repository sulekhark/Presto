﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelMEH : Rel
    {
        public RelMEH() : base(2, "MEH")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domM.GetName();
            domNames[1] = ProgramDoms.domEH.GetName();
        }

        public bool Add(MethodRefWrapper methW, ExHandlerWrapper ehW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domM.IndexOf(methW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domEH.IndexOf(ehW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
