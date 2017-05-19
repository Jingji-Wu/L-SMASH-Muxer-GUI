using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace L_SMASH___MP4_Muxer.Job
{
    class JobProcessor
    {
        private string _muxerPath;
        private long _totalFileSize;
        private Process proc;
        private ManualResetEvent mre = new ManualResetEvent(false);

        public delegate void MuxingProgressChangedEventHandler(double progress);
        public event MuxingProgressChangedEventHandler ProgressChanged;

        public delegate void MuxingLogEventHandler(string log);
        public event MuxingLogEventHandler LogChanged;

        public JobProcessor( string muxerPath)
        {
            _muxerPath = muxerPath;
            _totalFileSize = 0;
        }

        private async Task StartProcess(string filename, string arguments)
        {
            proc = new Process();
            var startInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = arguments,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Minimized,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            proc.StartInfo = startInfo;

            try
            {
                proc.OutputDataReceived += outputHandler;
                proc.ErrorDataReceived += outputHandler;
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                await Task.Run(() => proc.WaitForExit());
                mre.WaitOne();
                proc.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void outputHandler(object sender, DataReceivedEventArgs e)
        {
            string outputData = e.Data;
            if (!string.IsNullOrEmpty(outputData))
            {
                try
                {
                    LogChanged(outputData);
                    string[] lines = Regex.Split(outputData, "\r\n|\r|\n");
                    foreach (string line in lines)
                    {
                        Match progressMatch;
                        double progress = -1;

                        if (line.Contains("Importing: "))
                        {
                            progressMatch = Regex.Match(line, @"Importing: (\d*?) bytes", RegexOptions.Compiled);
                            if (progressMatch.Groups.Count < 2) return;
                            progress = Convert.ToDouble(double.Parse(progressMatch.Groups[1].Value) / _totalFileSize * 100d);
                        }
                        else if (line.Contains("Muxing completed"))
                        {
                            progress = 100;
                            mre.Set();
                        }
                        if (progress > -1 && ProgressChanged != null)
                        {
                            ProgressChanged(progress);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public void StartMuxer(MuxerJob inputJob)
        {
            string mainProgram = string.Empty;
            string args = string.Empty;
            mainProgram = _muxerPath;
            args = inputJob.GenerateMuxerArgs();
            _totalFileSize = inputJob.TotalSize;
            StartProcess(mainProgram, args);
        }
    }
}
