// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelClassT : Rel
    {
        public RelClassT() : base(1, "classT")
        {
            domNames = new string[1];
            domNames[0] = ProgramDoms.domT.GetName();
        }

        public bool Add(TypeRefWrapper typRefW)
        {
            int[] iarr = new int[1];

            iarr[0] = ProgramDoms.domT.IndexOf(typRefW);
            if (iarr[0] == -1) return false;
            return base.Add(iarr);
        }
    }
}
