// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers;

namespace Daffodil.DatalogAnalysisFW.ProgramFacts.Doms
{
    public class DomZ : Dom<IntWrapper>
    {
        public DomZ() : base("Z")
        {
            for (int i = 0; i < 32; i++)
            {
                base.Add(new IntWrapper(i));
            }
        }
    }
}
