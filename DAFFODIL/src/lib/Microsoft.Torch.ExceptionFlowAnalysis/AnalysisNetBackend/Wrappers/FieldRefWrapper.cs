using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole;
using Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts;



namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public class FieldRefWrapper : IWrapper
    {
        readonly IFieldReference fld;
        readonly string typeName;

        public FieldRefWrapper(IFieldReference fld)
        {
            this.fld = fld;
            typeName = (fld == null) ? "" : fld.ContainingType.FullName();
        }

        public override string ToString()
        {
            if (fld == null)
            {
                return "null";
            }
            else
            {
                return fld.Name.Value;
            }
        }

        public string GetDesc()
        {
            string s = "CLASS:" + typeName;
            return s;
        }
    }
}
