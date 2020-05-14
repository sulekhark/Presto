// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelReachableI : Rel
    {
        public RelReachableI() : base(1, "reachableI")
        {
            domNames = new string[1];
            domNames[0] = ProgramDoms.domI.GetName();
        }

        public bool Add(InstructionWrapper instW)
        {
            int[] iarr = new int[1];

            iarr[0] = ProgramDoms.domI.IndexOf(instW);
            if (iarr[0] == -1) return false;
            return base.Add(iarr);
        }
    }
}
