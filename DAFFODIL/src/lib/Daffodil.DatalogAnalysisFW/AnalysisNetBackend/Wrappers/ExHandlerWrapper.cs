// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

using Daffodil.DatalogAnalysisFW.ProgramFacts;

namespace Daffodil.DatalogAnalysisFW.AnalysisNetBackend.Wrappers
{
    public class ExHandlerWrapper : IWrapper
    {
        readonly InstructionWrapper instW;

        public ExHandlerWrapper(InstructionWrapper instW)
        {
            this.instW = instW;
        }

        public override string ToString()
        {
            return instW.ToString();
        }

        public string GetDesc()
        {
            return instW.GetDesc();
        }
    }
}
