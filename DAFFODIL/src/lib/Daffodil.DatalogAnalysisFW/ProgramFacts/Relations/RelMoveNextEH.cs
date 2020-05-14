// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelMoveNextEH : Rel
    {
        public RelMoveNextEH() : base(2, "MoveNextEH")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domM.GetName();
            domNames[1] = ProgramDoms.domEH.GetName();
        }

        public bool Add(MethodRefWrapper methW, ExHandlerWrapper ehW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domM.IndexOf(methW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domEH.IndexOf(ehW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
