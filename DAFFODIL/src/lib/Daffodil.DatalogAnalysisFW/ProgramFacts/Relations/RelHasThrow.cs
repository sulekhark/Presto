// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelHasThrow : Rel
    {
        public RelHasThrow() : base(2, "HasThrow")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domEH.GetName();
            domNames[1] = ProgramDoms.domP.GetName();
        }

        public bool Add(ExHandlerWrapper ehW, InstructionWrapper instW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domEH.IndexOf(ehW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domP.IndexOf(instW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
