﻿using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole;
using Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts;


namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public class MethodRefWrapper : IWrapper
    {
        IMethodReference methRef;
        MethodBody methBody;

        public MethodRefWrapper(IMethodReference methRef)
        {
            this.methRef = methRef;
            this.methBody = null;
        }

        public MethodRefWrapper(IMethodReference methRef, MethodBody methBody)
        {
            this.methRef = methRef;
            this.methBody = methBody;
        }

        public void Update (MethodBody methBody)
        {
            if (this.methBody == null)
            {
                this.methBody = methBody;
            }
            else if (methBody != this.methBody)
            {
                System.Console.WriteLine("WARNING: Updating method body more than once.");
            }
        }
        public override string ToString()
        {
            return methRef.FullName();
        }

        public string GetDesc()
        {
            string s = "CLASS:" + methRef.ContainingType.FullName();
            return s;
        }
    }
}
