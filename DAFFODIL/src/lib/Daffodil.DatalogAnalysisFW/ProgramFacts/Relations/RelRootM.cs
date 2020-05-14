// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelRootM : Rel
    {
        public RelRootM() : base(1, "rootM")
        {
            domNames = new string[1];
            domNames[0] = ProgramDoms.domM.GetName();
        }

        public bool Add(MethodRefWrapper methW)
        {
            int[] iarr = new int[1];

            iarr[0] = ProgramDoms.domM.IndexOf(methW);
            if (iarr[0] == -1) return false;
            return base.Add(iarr);
        }
    }
}
