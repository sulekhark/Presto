using System.Collections.Generic;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Values;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Model
{
    class VariableComparer : IEqualityComparer<IVariable>
    {
        public int GetHashCode(IVariable var)
        {
            return var.GetHashCode();
        }

        public bool Equals(IVariable x, IVariable y)
        {
            if (x == null || y == null) return false;
            return (x.Equals(y));
        }
    }
}
