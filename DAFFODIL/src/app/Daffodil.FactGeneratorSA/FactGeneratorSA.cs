using Daffodil.DatalogAnalysisFW.Common;
using Daffodil.DatalogAnalysisFW.ProgramFacts;
using Daffodil.DatalogAnalysisFW.AnalysisNetConsole;

namespace Daffodil.FactGeneratorSA
{
    class ExFlowAnalysisSA
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ConfigParams.LoadConfig(@"C:\Users\sulek\work\ExcAnalysis\DAFFODIL\DAFFODIL\src\test\E2EDemoR\daffodil.cfg");
                // ConfigParams.LoadConfig(@"C:\Users\sulek\work\ExcAnalysis\Test\storage-blob-dotnet-getting-started\BlobStorage\daffodil.cfg");
                // ConfigParams.LoadConfig(@"C:\Users\sulek\work\ExcAnalysis\Test\StorageBug3\StorageBug3\daffodil.cfg");
                // ConfigParams.LoadConfig(@"C:\Users\sulek\work\ExcAnalysis\FromGithub\bmk1\storage\good\azure-storage-net-data-movement\samples\DataMovementSamples\DataMovementSamples\daffodil.cfg");
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
