using Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public enum HeapAccKind
    {
        HeapObj,
        StructObj
    }
    public class HeapAccWrapper : IWrapper
    {
        InstructionWrapper instW;
        VariableWrapper varW;
        readonly HeapAccKind kind;


        public HeapAccWrapper(InstructionWrapper instW)
        {
            this.instW = instW;
            kind = HeapAccKind.HeapObj;
        }

        public HeapAccWrapper(VariableWrapper varW)
        {
            this.varW = varW;
            kind = HeapAccKind.StructObj;
        }
        public override string ToString()
        {
            if (kind == HeapAccKind.HeapObj)
            {
                return (instW.ToString());
            }
            else if (kind == HeapAccKind.StructObj)
            {
                return (varW.ToString());
            }
            else
            {
                return "UNK";
            }
        }

        public string GetDesc()
        {
            if (kind == HeapAccKind.HeapObj)
            {
                return (instW.GetDesc());
            }
            else if (kind == HeapAccKind.StructObj)
            {
                return (varW.GetDesc());
            }
            else
            {
                return "UNK";
            }
        }
    }
}

