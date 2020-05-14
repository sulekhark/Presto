// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelSub : Rel
    {
        public RelSub() : base(2, "sub")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domT.GetName();
            domNames[1] = ProgramDoms.domT.GetName();
        }

        public bool Add(TypeRefWrapper typRefW1, TypeRefWrapper typRefW2)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domT.IndexOf(typRefW1);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domT.IndexOf(typRefW2);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
