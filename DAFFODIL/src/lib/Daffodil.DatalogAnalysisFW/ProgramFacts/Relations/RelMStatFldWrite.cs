// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;


namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelMStatFldWrite : Rel
    {
        public RelMStatFldWrite() : base(3, "MStatFldWrite")
        {
            domNames = new string[3];
            domNames[0] = ProgramDoms.domM.GetName();
            domNames[1] = ProgramDoms.domF.GetName();
            domNames[2] = ProgramDoms.domV.GetName();
        }

        public bool Add(MethodRefWrapper mRefW, FieldRefWrapper fldW, VariableWrapper rhsW)
        {
            int[] iarr = new int[3];

            iarr[0] = ProgramDoms.domM.IndexOf(mRefW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domF.IndexOf(fldW);
            if (iarr[1] == -1) return false;
            iarr[2] = ProgramDoms.domV.IndexOf(rhsW);
            if (iarr[2] == -1) return false;
            return base.Add(iarr);
        }
    }
}
