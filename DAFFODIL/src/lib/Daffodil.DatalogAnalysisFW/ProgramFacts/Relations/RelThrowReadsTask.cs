﻿// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelThrowReadsTask : Rel
    {
        public RelThrowReadsTask() : base(2, "ThrowReadsTask")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domP.GetName();
            domNames[1] = ProgramDoms.domV.GetName();
        }

        public bool Add(InstructionWrapper throwStmtW, VariableWrapper varW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domP.IndexOf(throwStmtW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domV.IndexOf(varW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
