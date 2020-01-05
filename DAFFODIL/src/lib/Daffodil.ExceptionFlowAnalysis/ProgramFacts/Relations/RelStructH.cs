﻿using Daffodil.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;

namespace Daffodil.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelStructH : Rel
    {
        public RelStructH() : base(1, "structH")
        {
            domNames = new string[1];
            domNames[0] = ProgramDoms.domH.GetName();
        }

        public bool Add(HeapElemWrapper hpW)
        {
            int[] iarr = new int[1];

            iarr[0] = ProgramDoms.domH.IndexOf(hpW);
            if (iarr[0] == -1) return false;
            return base.Add(iarr);
        }
    }
}
