using Microsoft.Torch.ExceptionFlowAnalysis.Common;
using Microsoft.Torch.ExceptionFlowAnalysis.Z3Interface;
using Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole;

namespace Microsoft.Torch.ExFlowAnalysisSA
{
    class ExFlowAnalysisSA
    {
        static void Main()
        {
            ConfigParams.LoadConfig(@"C:\Users\torch\work\DAFFODIL\DAFFODIL\src\test\T4\daffodil.cfg");
            ProgramDoms.Initialize();
            ProgramRels.Initialize();
            ByteCodeAnalyzer.GenerateEDBFacts(@"C:\Users\torch\work\DAFFODIL\DAFFODIL\src\test\T4\bin\Debug\T4.exe");
            ProgramDoms.Save();
            ProgramRels.Save();
            Z3CommandLineInvoke.CopyFiles(ConfigParams.DatalogDir);
            Z3CommandLineInvoke.LaunchZ3("CIPtrAnalysis.datalog", ConfigParams.DatalogDir);
            // Z3CommandLineInvoke.LaunchZ3("ExcAnalysisIntraProc.datalog", ConfigParams.DatalogDir);     
            // Z3CommandLineInvoke.LaunchZ3("ExcAnalysisInterProc.datalog", ConfigParams.DatalogDir);         
            // Z3CommandLineInvoke.LaunchZ3("ExcFlows.datalog", ConfigParams.DatalogDir);
        }
    }
}
