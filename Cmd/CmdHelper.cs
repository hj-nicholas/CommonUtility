using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jane.Common.Utility.Cmd
{
    public class CmdHelper
    {
        private Process proc = null;
        public CmdHelper()
        {
            proc = new Process();
        }

        public void Run(string cmd)
        {
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            // proc.OutputDataReceived += new DataReceivedEventHandler(sortProcess_OutputDataReceived);
            proc.Start();
            StreamWriter cmdWriter = proc.StandardInput;
            proc.BeginOutputReadLine();
            if (!String.IsNullOrEmpty(cmd))
            {
                cmdWriter.WriteLine(cmd);
            }
            cmdWriter.Close();
            proc.Close();
        }
    }
}
