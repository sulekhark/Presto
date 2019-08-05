using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public class MethodRefWrapper
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
    }
}
