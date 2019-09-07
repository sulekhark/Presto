using System.Collections.Generic;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Instructions;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Model
{
    class InstructionComparer : IEqualityComparer<Instruction>
    {
        public int GetHashCode(Instruction inst)
        {
            return inst.GetHashCode();
        }

        public bool Equals(Instruction x, Instruction y)
        {
            if (x == null || y == null) return false;
            return (x.Equals(y));
        }
    }
}
