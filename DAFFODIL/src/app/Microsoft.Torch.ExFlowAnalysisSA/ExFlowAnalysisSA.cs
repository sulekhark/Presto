using Microsoft.Torch.ExceptionFlowAnalysis.Common;
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
                // ConfigParams.LoadConfig(@"C:\Users\sulek\work\ExcAnalysis\DAFFODIL\DAFFODIL\src\test\T4\daffodil.cfg");
                // ConfigParams.LoadConfig(@"C:\Users\sulek\work\ExcAnalysis\Test\storage-blob-dotnet-getting-started\BlobStorage\daffodil.cfg");
                // ConfigParams.LoadConfig(@"C:\Users\sulek\work\ExcAnalysis\Test\StorageBug3\StorageBug3\daffodil.cfg");
                ConfigParams.LoadConfig(@"C:\Users\sulek\work\ExcAnalysis\FromGithub\bmk1\storage\good\azure-storage-net-data-movement\samples\DataMovementSamples\DataMovementSamples\daffodil.cfg");
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
        }
    }
}
