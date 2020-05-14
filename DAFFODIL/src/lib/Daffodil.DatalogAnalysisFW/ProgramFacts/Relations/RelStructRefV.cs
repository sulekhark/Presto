// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelStructRefV : Rel
    {
        public RelStructRefV() : base(1, "structRefV")
        {
            domNames = new string[1];
            domNames[0] = ProgramDoms.domV.GetName();
        }

        public bool Add(VariableWrapper varW)
        {
            int[] iarr = new int[1];

            iarr[0] = ProgramDoms.domV.IndexOf(varW);
            if (iarr[0] == -1) return false;
            return base.Add(iarr);
        }
    }
}
