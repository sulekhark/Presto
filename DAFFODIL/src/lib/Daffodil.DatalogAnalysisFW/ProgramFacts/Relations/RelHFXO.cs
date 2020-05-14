// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelHFXO : Rel
    {
        public RelHFXO() : base(3, "HFXO")
        {
            domNames = new string[3];
            domNames[0] = ProgramDoms.domH.GetName();
            domNames[1] = ProgramDoms.domF.GetName();
            domNames[2] = ProgramDoms.domX.GetName();
        }

        public bool Add(HeapElemWrapper allocW, FieldRefWrapper fldRefW, AddressWrapper addrW)
        {
            int[] iarr = new int[3];

            iarr[0] = ProgramDoms.domH.IndexOf(allocW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domF.IndexOf(fldRefW);
            if (iarr[1] == -1) return false;
            iarr[2] = ProgramDoms.domX.IndexOf(addrW);
            if (iarr[2] == -1) return false;
            return base.Add(iarr);
        }
    }
}
