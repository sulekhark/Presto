// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelNestedEH : Rel
    {
        public RelNestedEH() : base(3, "NestedEH")
        {
            domNames = new string[3];
            domNames[0] = ProgramDoms.domM.GetName();
            domNames[1] = ProgramDoms.domEH.GetName();
            domNames[2] = ProgramDoms.domEH.GetName();
        }

        public bool Add(MethodRefWrapper methW, ExHandlerWrapper ehW1, ExHandlerWrapper ehW2)
        {
            int[] iarr = new int[3];

            iarr[0] = ProgramDoms.domM.IndexOf(methW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domEH.IndexOf(ehW1);
            if (iarr[1] == -1) return false;
            iarr[2] = ProgramDoms.domEH.IndexOf(ehW2);
            if (iarr[2] == -1) return false;
            return base.Add(iarr);
        }
    }
}
