﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelHasMethInvk : Rel
    {
        public RelHasMethInvk() : base(2, "HasMethInvk")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domEH.GetName();
            domNames[1] = ProgramDoms.domI.GetName();
        }

        public bool Add(ExHandlerWrapper ehW, InstructionWrapper invkW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domEH.IndexOf(ehW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domI.IndexOf(invkW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
