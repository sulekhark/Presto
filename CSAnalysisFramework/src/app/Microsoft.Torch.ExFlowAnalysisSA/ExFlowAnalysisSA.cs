using System;
using System.IO;
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
            ConfigParams.Z3ExePath = @"C:\Users\t-sukulk\source\repos\CSAnalysisFramework\CSAnalysisFramework\packages\Z3-4.1\bin\z3.exe";
            ConfigParams.StubsPath = @"C:\Users\t-sukulk\source\repos\CSAnalysisFramework\CSAnalysisFramework\src\stubs\Microsoft.Torch.Stubs\bin\Debug\Microsoft.Torch.Stubs.dll";
            ProgramDoms.Initialize();
            ProgramRels.Initialize();
            // ByteCodeAnalyzer.GenerateEDBFacts(@"C:\Users\t-sukulk\work\FromGithub\bmk\azure-storage-net\Lib\Common.Split\NetFx\bin\Debug\Microsoft.Azure.Storage.Common.dll");
            // ByteCodeAnalyzer.GenerateEDBFacts(@"C:\Users\t-sukulk\work\FromGithub\bmk\azure-storage-net\Lib\WindowsDesktop.Split\File\bin\Debug\Microsoft.Azure.Storage.File.dll");

            ConfigParams.DatalogDir = @"C:\Users\t-sukulk\source\repos\CSAnalysisFramework\CSAnalysisFramework\src\test\T12\temp";
            ByteCodeAnalyzer.GenerateEDBFacts(@"C:\Users\t-sukulk\source\repos\CSAnalysisFramework\CSAnalysisFramework\src\test\T12\bin\Debug\T12.exe");

            // ConfigParams.DatalogDir = @"C:\Users\t-sukulk\work\Test\storage-blob-dotnet-getting-started\BlobStorage\temp";
            // ByteCodeAnalyzer.GenerateEDBFacts(@"C:\Users\t-sukulk\work\Test\storage-blob-dotnet-getting-started\BlobStorage\bin\Debug\BlobStorage.exe");

            ProgramDoms.Save();
            ProgramRels.Save();
            Z3CommandLineInvoke z3Cmd = new Z3CommandLineInvoke();
            z3Cmd.RunAnalysis(@"C:\Users\t-sukulk\source\repos\CSAnalysisFramework\CSAnalysisFramework\src\lib\Microsoft.Torch.ExceptionFlowAnalysis\PtrAndExcAnalysis\CIPtrAnalysis.datalog");
        }
    }
}
