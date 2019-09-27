﻿using Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
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
