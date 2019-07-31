using Microsoft.Cci;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public class FieldRefWrapper
    {
        IFieldReference fld;

        public FieldRefWrapper(IFieldReference fld)
        {
            this.fld = fld;
        }

        public override string ToString()
        {
            if (fld == null)
            {
                return "null";
            }
            else
            {
                return fld.ToString();
            }
        }
    }
}
