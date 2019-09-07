using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Torch.ExceptionFlowAnalysis.Common;

namespace Microsoft.Torch.ExceptionFlowAnalysis.Z3Interface
{
    public class Z3CommandLineInvoke
    {
        Process processCp;
        Process processZ3;

        public Z3CommandLineInvoke()
        {
            processCp = new Process();
            processZ3 = new Process();
        }

        public void RunAnalysis(string analysisToRun)
        {
            CopyFile(analysisToRun, ConfigParams.DatalogDir);
            string analysisName = Path.GetFileName(analysisToRun);
            LaunchZ3(analysisName, ConfigParams.DatalogDir);
        }

        private void CopyFile(string filePath, string destDir)
        {
            processCp.EnableRaisingEvents = true;
            processCp.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(CopyOutputDataReceived);
            processCp.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(CopyErrorDataReceived);
            processCp.Exited += new System.EventHandler(CopyExited);

            processCp.StartInfo.FileName = "cmd.exe";
            processCp.StartInfo.UseShellExecute = false;
            processCp.StartInfo.RedirectStandardInput = true;
            processCp.StartInfo.RedirectStandardError = true;
            processCp.StartInfo.RedirectStandardOutput = true;

            processCp.Start();
            processCp.BeginErrorReadLine();
            processCp.BeginOutputReadLine();
            using (StreamWriter sw = processCp.StandardInput)
            {
                sw.WriteLine("copy /Y " + filePath + " " + destDir);
            }
            //We want a blocking call
            processCp.WaitForExit();
        }

        private void CopyExited(object sender, EventArgs e)
        {
            Console.WriteLine(string.Format("Copy exited with code {0}\n", processCp.ExitCode.ToString()));
        }

        private void CopyErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Data))
            {
                System.Console.WriteLine(e.Data);
            }
        }

        private void CopyOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Data))
            {
                System.Console.WriteLine(e.Data);
            }
        }

        private void LaunchZ3(string analysisToRun, string executionDir)
        {
            processZ3.EnableRaisingEvents = true;
            processZ3.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(Z3OutputDataReceived);
            processZ3.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(Z3ErrorDataReceived);
            processZ3.Exited += new System.EventHandler(Z3Exited);

            
            processZ3.StartInfo.FileName = ConfigParams.Z3ExePath;
            processZ3.StartInfo.Arguments = analysisToRun;
            processZ3.StartInfo.UseShellExecute = false;
            processZ3.StartInfo.RedirectStandardError = true;
            processZ3.StartInfo.RedirectStandardOutput = true;
            processZ3.StartInfo.WorkingDirectory = executionDir;

            processZ3.Start();
            processZ3.BeginErrorReadLine();
            // Raises the OutputDataReceived event for each line of output
            processZ3.BeginOutputReadLine();
            //We want a blocking call (at present)
            processZ3.WaitForExit();
        }

        private void Z3Exited(object sender, EventArgs e)
        {
            Console.WriteLine(string.Format("Z3 exited with code {0}\n", processZ3.ExitCode.ToString()));
        }

        private void Z3ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Data))
            {
                System.Console.WriteLine(e.Data);
                Z3OutputParser.ParseZ3Error(e.Data);
            }
        }

        private void Z3OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e.Data))
            {
                System.Console.WriteLine(e.Data);
                Z3OutputParser.ParseZ3Output(e.Data);
            }
        }
    }
}
