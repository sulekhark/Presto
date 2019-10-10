using Microsoft.Torch.ExceptionFlowAnalysis.Common;
using Microsoft.Torch.ExceptionFlowAnalysis.Z3Interface;
using Microsoft.Torch.ExceptionFlowAnalysis.ProgramFacts;
using Microsoft.Torch.ExceptionFlowAnalysis.AnalysisNetConsole;

namespace Microsoft.Torch.ExFlowAnalysisSA
{
    class ExFlowAnalysisSA
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ConfigParams.LoadConfig(@"C:\Users\torch\work\DAFFODIL\DAFFODIL\src\test\T4\daffodil.cfg");
            }
            else if (args.Length == 1)
            {
                ConfigParams.LoadConfig(args[0]);
            }
            else
            {
                System.Console.WriteLine("Wrong number of input args. Exiting.");
                System.Environment.Exit(1);
            }
            ProgramDoms.Initialize();
            ProgramRels.Initialize();
            ByteCodeAnalyzer.GenerateEDBFacts(ConfigParams.AssemblyPath);
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
