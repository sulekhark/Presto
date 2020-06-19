// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelMethSetsTask : Rel
    {
        public RelMethSetsTask() : base(3, "MethSetsTask")
        {
            domNames = new string[3];
            domNames[0] = ProgramDoms.domM.GetName();
            domNames[1] = ProgramDoms.domV.GetName();
            domNames[2] = ProgramDoms.domEH.GetName();
        }

        public bool Add(MethodRefWrapper methW, VariableWrapper varW, ExHandlerWrapper ehW)
        {
            int[] iarr = new int[3];

            iarr[0] = ProgramDoms.domM.IndexOf(methW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domV.IndexOf(varW);
            if (iarr[1] == -1) return false;
            iarr[2] = ProgramDoms.domEH.IndexOf(ehW);
            if (iarr[2] == -1) return false;
            return base.Add(iarr);
        }
    }
}
