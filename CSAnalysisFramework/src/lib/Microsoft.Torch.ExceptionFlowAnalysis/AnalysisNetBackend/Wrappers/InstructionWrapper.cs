using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Instructions;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public class InstructionWrapper
    {
        readonly string instStr;

        public InstructionWrapper(Instruction inst)
        {
            instStr = inst.ToString();
        }

        public override string ToString()
        {
            return instStr;
        }
    }
}
