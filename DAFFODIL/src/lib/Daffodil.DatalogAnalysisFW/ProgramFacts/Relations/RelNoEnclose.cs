﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelNoEnclose : Rel
    {
        public RelNoEnclose() : base(2, "NoEnclose")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domM.GetName();
            domNames[1] = ProgramDoms.domP.GetName();
        }

        public bool Add(MethodRefWrapper mRefW, InstructionWrapper instW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domM.IndexOf(mRefW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domP.IndexOf(instW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
