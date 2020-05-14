// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;


namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelStaticTM : Rel
    {
        public RelStaticTM() : base(2, "staticTM")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domT.GetName();
            domNames[1] = ProgramDoms.domM.GetName();
        }

        public bool Add(TypeRefWrapper typRefW, MethodRefWrapper mRefW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domT.IndexOf(typRefW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domM.IndexOf(mRefW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
