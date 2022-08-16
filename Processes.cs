﻿using System.Diagnostics;

namespace DoorDownloader {
    internal class Processes {
        public static string pythonOverridePath = "";

        public static Process StartProcessWithOptions(string processPath, string? arguments = null, string? workingDirectory = null) {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = processPath;
            if (arguments != null)
                processStartInfo.Arguments = arguments;
            if (workingDirectory != null)
                processStartInfo.WorkingDirectory = workingDirectory;

            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;
            Process? process;
            try {
                process = Process.Start(processStartInfo);
            } catch (Exception) {
                process = null;
            }
            return process;
        }

        public static Process StartPythonWithOptions(string arguments, string? workingDirectory = null) {
            List<string> pythons = new List<string> {
                "python3",
                "python",
                "py",
                AppContext.BaseDirectory + "python/" + "python"
            };

            if (Processes.pythonOverridePath != null && Processes.pythonOverridePath.Length > 0) {
                if (Program.debug)
                    Console.WriteLine("Using custom python: '" + Processes.pythonOverridePath + "'");
                pythons.Clear();
                pythons.Add(Processes.pythonOverridePath);
            }

            Process python;

            foreach(string pythonPath in pythons) {
                string finalPath = pythonPath;
                if (OperatingSystem.IsWindows())
                    finalPath += ".exe";

                python = StartProcessWithOptions(finalPath, arguments, workingDirectory);
                if (python != null && python.StandardError.ReadToEnd().Length < 1)
                    return python;
                if (Program.debug)
                    Console.WriteLine("Failed to launch using `" + finalPath + "`, trying next");
            }

            throw new Exception("Cannot get Python running!");
        }

        public static string GetPyVersion() {
            Process? process;
            try {
                process = StartPythonWithOptions(" -c \"import sys; print(str(sys.version_info[0])+'.'+str(sys.version_info[1]))\"");
            } catch (Exception) {
                return "";
            }
            
            return process.StandardOutput.ReadToEnd().Trim();
        }
    }
}
