// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Relations
{
    public class RelTaskFldInBuilder : Rel
    {
        public RelTaskFldInBuilder() : base(2, "TaskFldInBuilder")
        {
            domNames = new string[2];
            domNames[0] = ProgramDoms.domT.GetName();
            domNames[1] = ProgramDoms.domF.GetName();
        }

        public bool Add(TypeRefWrapper typeW, FieldRefWrapper fldW)
        {
            int[] iarr = new int[2];

            iarr[0] = ProgramDoms.domT.IndexOf(typeW);
            if (iarr[0] == -1) return false;
            iarr[1] = ProgramDoms.domF.IndexOf(fldW);
            if (iarr[1] == -1) return false;
            return base.Add(iarr);
        }
    }
}
