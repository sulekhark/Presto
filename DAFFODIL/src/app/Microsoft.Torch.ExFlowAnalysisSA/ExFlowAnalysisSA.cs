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
            ConfigParams.Z3ExePath = @"F:\DAFFODIL\DAFFODIL\packages\Z3-4.1\bin\z3.exe";
            ConfigParams.StubsPath = @"F:\DAFFODIL\DAFFODIL\src\stubs\Microsoft.Torch.Stubs\bin\Debug\Microsoft.Torch.Stubs.dll";
            ProgramDoms.Initialize();
            ProgramRels.Initialize();
            // ByteCodeAnalyzer.GenerateEDBFacts(@"F:\FromGithub\bmk\azure-storage-net\Lib\Common.Split\NetFx\bin\Debug\Microsoft.Azure.Storage.Common.dll");
            // ByteCodeAnalyzer.GenerateEDBFacts(@"F:\FromGithub\bmk\azure-storage-net\Lib\WindowsDesktop.Split\File\bin\Debug\Microsoft.Azure.Storage.File.dll");

            ConfigParams.DatalogDir = @"F:\DAFFODIL\DAFFODIL\src\test\T13\temp";
            ByteCodeAnalyzer.GenerateEDBFacts(@"F:\DAFFODIL\DAFFODIL\src\test\T13\bin\Debug\T13.exe");

            // ConfigParams.DatalogDir = @"F:\Test\storage-blob-dotnet-getting-started\BlobStorage\temp";
            // ByteCodeAnalyzer.GenerateEDBFacts(@"F:\Test\storage-blob-dotnet-getting-started\BlobStorage\bin\Debug\BlobStorage.exe");

            ProgramDoms.Save();
            ProgramRels.Save();
           
            Z3CommandLineInvoke z3Cmd1 = new Z3CommandLineInvoke();
            z3Cmd1.RunAnalysis(@"F:\DAFFODIL\DAFFODIL\src\lib\Microsoft.Torch.ExceptionFlowAnalysis\PtrAndExcAnalysis\CIPtrAnalysis.datalog");
            Z3CommandLineInvoke z3Cmd2 = new Z3CommandLineInvoke();
            z3Cmd2.RunAnalysis(@"F:\DAFFODIL\DAFFODIL\src\lib\Microsoft.Torch.ExceptionFlowAnalysis\PtrAndExcAnalysis\ExcAnalysisIntraProc.datalog");
            Z3CommandLineInvoke z3Cmd3 = new Z3CommandLineInvoke();
            z3Cmd3.RunAnalysis(@"F:\DAFFODIL\DAFFODIL\src\lib\Microsoft.Torch.ExceptionFlowAnalysis\PtrAndExcAnalysis\ExcAnalysisInterProc.datalog");
            Z3CommandLineInvoke z3Cmd4 = new Z3CommandLineInvoke();
            z3Cmd4.RunAnalysis(@"F:\DAFFODIL\DAFFODIL\src\lib\Microsoft.Torch.ExceptionFlowAnalysis\PtrAndExcAnalysis\ExcFlows.datalog");
           
        }
    }
}
