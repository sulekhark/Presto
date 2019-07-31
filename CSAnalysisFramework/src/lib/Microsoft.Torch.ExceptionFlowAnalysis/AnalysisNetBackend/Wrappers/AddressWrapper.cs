using Microsoft.Cci;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Values;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public enum AddressKind
    {
        AddrM,
        AddrHF,
        AddrF,
        AddrV
    }
    public class AddressWrapper
    {
        MethodRefWrapper mRefW;
        TypeRefWrapper typeRefW;
        FieldRefWrapper fldRefW;
        VariableWrapper varW;
        readonly AddressKind kind;


        public AddressWrapper(MethodRefWrapper mRefW)
        {
            this.mRefW = mRefW;
            kind = AddressKind.AddrM;
        }

        public AddressWrapper(TypeRefWrapper typeRefW, FieldRefWrapper fldRefW)
        {
            this.typeRefW = typeRefW;
            this.fldRefW = fldRefW;
            kind = AddressKind.AddrHF;
        }

        public AddressWrapper(FieldRefWrapper fldRefW)
        {
            this.fldRefW = fldRefW;
            kind = AddressKind.AddrF;
        }

        public AddressWrapper(VariableWrapper varW)
        {
            this.varW = varW;
            kind = AddressKind.AddrV;
        }
        public override string ToString()
        {
            if (kind == AddressKind.AddrM)
            {
                return mRefW.ToString();
            }
            else if (kind == AddressKind.AddrHF)
            {
                return (typeRefW.ToString() + "::" + fldRefW.ToString());
            }
            else if (kind == AddressKind.AddrF)
            {
                return (fldRefW.ToString());
            }
            else if (kind == AddressKind.AddrV)
            {
                return (varW.ToString());
            }
            else
            {
                return "unknown address entry";
            }
        }
    }
}
