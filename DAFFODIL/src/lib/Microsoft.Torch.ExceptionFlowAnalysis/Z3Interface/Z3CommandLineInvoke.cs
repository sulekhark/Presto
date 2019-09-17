using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Torch.ExceptionFlowAnalysis.Common;

namespace Microsoft.Torch.ExceptionFlowAnalysis.Z3Interface
{
    public static class Z3CommandLineInvoke
    {
        public static void CopyFiles(string destDir)
        {
            Process pr1 = new Process();
            CopyFile(pr1, Path.Combine(ConfigParams.AnalysesPath, "CIPtrAnalysis.datalog"), destDir);
            Process pr2 = new Process();
            CopyFile(pr2, Path.Combine(ConfigParams.AnalysesPath, "ExcAnalysisIntraProc.datalog"), destDir);
            Process pr3 = new Process();
            CopyFile(pr3, Path.Combine(ConfigParams.AnalysesPath, "ExcAnalysisInterProc.datalog"), destDir);
            Process pr4 = new Process();
            CopyFile(pr4, Path.Combine(ConfigParams.AnalysesPath, "ExcFlows.datalog"), destDir);
            Process pr5 = new Process();
            CopyFile(pr5, Path.Combine(ConfigParams.AnalysesPath, "parse_z3_out.py"), destDir);
            Process pr6 = new Process();
            CopyFile(pr6, Path.Combine(ConfigParams.AnalysesPath, "run_all.sh"), destDir);
        }

        private static void CopyFile(Process pr, string filePath, string destDir)
        {
            pr.EnableRaisingEvents = true;
            pr.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(CopyOutputDataReceived);
            pr.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(CopyErrorDataReceived);
            pr.Exited += new System.EventHandler(CopyExited);

            pr.StartInfo.FileName = "cmd.exe";
            pr.StartInfo.UseShellExecute = false;
            pr.StartInfo.RedirectStandardInput = true;
            pr.StartInfo.RedirectStandardError = true;
            pr.StartInfo.RedirectStandardOutput = true;

            pr.Start();
            pr.BeginErrorReadLine();
            pr.BeginOutputReadLine();
            using (StreamWriter sw = pr.StandardInput)
            {
                sw.WriteLine("copy /Y " + filePath + " " + destDir);
            }
            //We want a blocking call
            pr.WaitForExit();
        }

        private static void CopyExited(object sender, EventArgs e)
        {
            Console.WriteLine(string.Format("Copy exited\n"));
        }

        private static void CopyErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Data))
            {
                System.Console.WriteLine(e.Data);
            }
        }

        private static void CopyOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Data))
            {
                System.Console.WriteLine(e.Data);
            }
        }

        public static void LaunchZ3(string analysisToRun, string executionDir)
        {
            Process pr = new Process();
            pr.EnableRaisingEvents = true;
            pr.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(Z3OutputDataReceived);
            pr.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(Z3ErrorDataReceived);
            pr.Exited += new System.EventHandler(Z3Exited);

            
            pr.StartInfo.FileName = ConfigParams.Z3ExePath;
            pr.StartInfo.Arguments = analysisToRun;
            pr.StartInfo.UseShellExecute = false;
            pr.StartInfo.RedirectStandardError = true;
            pr.StartInfo.RedirectStandardOutput = true;
            pr.StartInfo.WorkingDirectory = executionDir;

            pr.Start();
            pr.BeginErrorReadLine();
            // Raises the OutputDataReceived event for each line of output
            pr.BeginOutputReadLine();
            //We want a blocking call (at present)
            pr.WaitForExit();
        }

        private static void Z3Exited(object sender, EventArgs e)
        {
            Console.WriteLine(string.Format("Z3 exited\n"));
        }

        private static void Z3ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Data))
            {
                System.Console.WriteLine(e.Data);
                Z3OutputParser.ParseZ3Error(e.Data);
            }
        }

        private static void Z3OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Data))
            {
                System.Console.WriteLine(e.Data);
                Z3OutputParser.ParseZ3Output(e.Data);
            }
        }
    }
}
