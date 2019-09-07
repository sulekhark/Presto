
namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public class ExHandlerWrapper
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
    }
}
