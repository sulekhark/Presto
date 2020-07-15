// Copyright (c) Sulekha Kulkarni.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.

﻿using Daffodil.DatalogAnalysisFW.Common;
using Daffodil.DatalogAnalysisFW.ProgramFacts;
using Daffodil.DatalogAnalysisFW.AnalysisNetConsole;
using System.IO;

namespace Daffodil.FactGeneratorSA
{
    class ExFlowAnalysisSA
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ConfigParams.LoadConfig(@"C:\Users\sulek\work\ExcAnalysis\Presto\DAFFODIL\src\test\ScrambledSquares\daffodil.cfg");
                // ConfigParams.LoadConfig(@"C:\Users\sulek\work\ExcAnalysis\Presto\DAFFODIL\src\test\T5\daffodil.cfg");
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
            string fPath = Path.Combine(ConfigParams.DatalogDir, "source_root.txt");
            StreamWriter sw = new StreamWriter(fPath);
            sw.WriteLine(ConfigParams.SourceRoot);
            sw.Close();
        }
    }
}
