using Daffodil.ExceptionFlowAnalysis.AnalysisNetBackend.ThreeAddressCode.Instructions;
using Daffodil.ExceptionFlowAnalysis.AnalysisNetConsole;
using Daffodil.ExceptionFlowAnalysis.ProgramFacts;

using Microsoft.Cci;

namespace Daffodil.ExceptionFlowAnalysis.AnalysisNetBackend.Wrappers
{
    public class InstructionWrapper : IWrapper
    {
        readonly string instStr;
        readonly int srcLine;
        readonly string srcFile;
        readonly string className;
        readonly string methodName;

        public InstructionWrapper(Instruction inst, IMethodDefinition meth)
        {
            instStr = inst.ToString();
            methodName = meth.FullName();
            className = meth.ContainingTypeDefinition.FullName();
            if (inst.Location != null)
            {
                srcLine = inst.Location.StartLine;
                srcFile = inst.Location.PrimarySourceDocument.Location;
            }
            else
            {
                srcLine = -1;
                srcFile = "NA";
            }
        }

        public override string ToString()
        {
            return instStr;
        }

        public string GetDesc()
        {
            string s = "CLASS:" + className + " METHOD:" + methodName + " SRCFILE:" + srcFile + " SRCLINE:" + srcLine;
            return s;
        }
    }
}
