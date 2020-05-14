// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;


namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelDelegateIV : Rel
    {
        public RelDelegateIV() : base(2, "DelegateIV")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domI.GetName();
            domNames[1] = ProgramDoms.domV.GetName();
        }

        public bool Add(InstructionWrapper invkW, VariableWrapper varW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domI.IndexOf(invkW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domV.IndexOf(varW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
