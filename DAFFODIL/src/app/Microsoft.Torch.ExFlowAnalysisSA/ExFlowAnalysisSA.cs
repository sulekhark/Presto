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
            ConfigParams.Z3ExePath = @"C:\Users\torch\work\DAFFODIL\DAFFODIL\packages\Z3-4.1\bin\z3.exe";
            ConfigParams.StubsPath = @"C:\Users\torch\work\DAFFODIL\DAFFODIL\src\stubs\Microsoft.Torch.Stubs\bin\Debug\Microsoft.Torch.Stubs.dll";
            ConfigParams.AnalysesPath = @"C:\Users\torch\work\DAFFODIL\DAFFODIL\src\lib\Microsoft.Torch.ExceptionFlowAnalysis\PtrAndExcAnalysis";
            ProgramDoms.Initialize();
            ProgramRels.Initialize();

            ConfigParams.LoadSavedScope = true;
            // ConfigParams.DatalogDir = @"C:\Users\torch\work\FromGithub\bmk1\storage\good\storage\src\Azure\Storage.Net.Microsoft.Azure.Storage\temp";
            // ConfigParams.LogDir = @"C:\Users\torch\work\FromGithub\bmk1\storage\good\storage\src\Azure\Storage.Net.Microsoft.Azure.Storage\logs";
            // ByteCodeAnalyzer.GenerateEDBFacts(@"C:\Users\torch\work\FromGithub\bmk1\storage\good\storage\src\Azure\Storage.Net.Microsoft.Azure.Storage\bin\Debug\net452\Storage.Net.Microsoft.Azure.Storage.dll");

            ConfigParams.DatalogDir = @"C:\Users\torch\work\DAFFODIL\DAFFODIL\src\test\T11\temp";
            ConfigParams.SaveScopePath = @"C:\Users\torch\work\DAFFODIL\DAFFODIL\src\test\T11\temp";
            ConfigParams.LogDir = @"C:\Users\torch\work\DAFFODIL\DAFFODIL\src\test\T11\logs";
            ByteCodeAnalyzer.GenerateEDBFacts(@"C:\Users\torch\work\DAFFODIL\DAFFODIL\src\test\T11\bin\Debug\T11.exe");

            // ConfigParams.DatalogDir = @"C:\Users\torch\work\Test\storage-blob-dotnet-getting-started\BlobStorage\temp";
            // ConfigParams.SaveScopePath = @"C:\Users\torch\work\Test\storage-blob-dotnet-getting-started\BlobStorage\temp";
            // ConfigParams.LogDir = @"C:\Users\torch\work\Test\storage-blob-dotnet-getting-started\BlobStorage\logs";
            // ByteCodeAnalyzer.GenerateEDBFacts(@"C:\Users\torch\work\Test\storage-blob-dotnet-getting-started\BlobStorage\bin\Debug\BlobStorage.exe");

            ProgramDoms.Save();
            ProgramRels.Save();
           
            Z3CommandLineInvoke.CopyFiles(ConfigParams.DatalogDir);
            Z3CommandLineInvoke.LaunchZ3("CIPtrAnalysis.datalog", ConfigParams.DatalogDir);
            Z3CommandLineInvoke.LaunchZ3("ExcAnalysisIntraProc.datalog", ConfigParams.DatalogDir);     
            Z3CommandLineInvoke.LaunchZ3("ExcAnalysisInterProc.datalog", ConfigParams.DatalogDir);         
            Z3CommandLineInvoke.LaunchZ3("ExcFlows.datalog", ConfigParams.DatalogDir);
        }
    }
}
