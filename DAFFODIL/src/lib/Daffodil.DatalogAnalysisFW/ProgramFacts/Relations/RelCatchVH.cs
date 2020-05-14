// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelCatchVH : Rel
    {
        public RelCatchVH() : base(2, "CatchVH")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domV.GetName();
            domNames[1] = ProgramDoms.domH.GetName();
        }

        public bool Add(VariableWrapper varW, HeapElemWrapper allocW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domV.IndexOf(varW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domH.IndexOf(allocW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
