﻿using Daffodil.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers;


namespace Daffodil.ExceptionFlowAnalysis.ProgramFacts.Relations
{
    public class RelMInstFldRead : Rel
    {
        public RelMInstFldRead() : base(4, "MInstFldRead")
        {
            domNames = new string[4];
            domNames[0] = ProgramDoms.domM.GetName();
            domNames[1] = ProgramDoms.domV.GetName();
            domNames[2] = ProgramDoms.domV.GetName();
            domNames[3] = ProgramDoms.domF.GetName();
        }

        public bool Add(MethodRefWrapper mRefW, VariableWrapper lhsW, VariableWrapper rhsW, FieldRefWrapper fldW)
        {
            int[] iarr = new int[4];

            iarr[0] = ProgramDoms.domM.IndexOf(mRefW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domV.IndexOf(lhsW);
            if (iarr[1] == -1) return false;
            iarr[2] = ProgramDoms.domV.IndexOf(rhsW);
            if (iarr[2] == -1) return false;
            iarr[3] = ProgramDoms.domF.IndexOf(fldW);
            if (iarr[3] == -1) return false;
            return base.Add(iarr);
        }
    }
}
