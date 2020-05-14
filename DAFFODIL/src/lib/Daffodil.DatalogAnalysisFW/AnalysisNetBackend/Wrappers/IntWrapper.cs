// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

using Daffodil.DatalogAnalysisFW.ProgramFacts;

namespace Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers
{
    public class IntWrapper : IWrapper
    {
        int val;

        public IntWrapper(int n)
        {
            val = n;
        }

        public override string ToString()
        {
            return val.ToString();
        }

        public string GetDesc()
        {
            return "";
        }
    }
}
