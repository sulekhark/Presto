using Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts;

namespace Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public enum HeapElemKind
    {
        RefObj,
        StructObj,
        ArrElemStructObj
    }
    public class HeapElemWrapper : IWrapper
    {
        InstructionWrapper instW;
        VariableWrapper varW;
        readonly HeapElemKind kind;


        public HeapElemWrapper(InstructionWrapper instW, bool createArrayElem)
        {
            this.instW = instW;
            kind = createArrayElem ? HeapElemKind.ArrElemStructObj : HeapElemKind.RefObj;
        }

        public HeapElemWrapper(VariableWrapper varW)
        {
            this.varW = varW;
            kind = HeapElemKind.StructObj;
        }
        public override string ToString()
        {
            if (kind == HeapElemKind.RefObj)
            {
                return (instW.ToString());
            }
            else if (kind == HeapElemKind.StructObj)
            {
                return (varW.ToString());
            }
            else if (kind == HeapElemKind.ArrElemStructObj)
            {
                return (instW.ToString() + " ARRAY_ELEMENT");
            }
            else
            {
                return "UNK";
            }
        }

        public string GetDesc()
        {
            if (kind == HeapElemKind.RefObj)
            {
                return (instW.GetDesc());
            }
            else if (kind == HeapElemKind.StructObj)
            {
                return (varW.GetDesc());
            }
            else if (kind == HeapElemKind.ArrElemStructObj)
            {
                return (instW.GetDesc());
            }
            else
            {
                return "UNK";
            }
        }
    }
}

