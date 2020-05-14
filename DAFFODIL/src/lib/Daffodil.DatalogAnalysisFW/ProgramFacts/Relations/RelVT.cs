// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;


namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelVT : Rel
    {
        public RelVT() : base(2, "VT")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domV.GetName();
            domNames[1] = ProgramDoms.domT.GetName();
        }

        public bool Add(VariableWrapper varW, TypeRefWrapper typRefW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domV.IndexOf(varW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domT.IndexOf(typRefW);
            if (iarr[1] == -1) return false;

            return base.Add(iarr);
        }
    }
}
