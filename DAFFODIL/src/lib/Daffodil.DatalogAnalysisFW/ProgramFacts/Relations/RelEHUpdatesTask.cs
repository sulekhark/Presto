﻿// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelEHUpdatesTask : Rel
    {
        public RelEHUpdatesTask() : base(2, "EHUpdatesTask")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domEH.GetName();
            domNames[1] = ProgramDoms.domH.GetName();
        }

        public bool Add(ExHandlerWrapper ehW, HeapElemWrapper allocW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domEH.IndexOf(ehW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domH.IndexOf(allocW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}