using System.Diagnostics;

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
            string pythonPath = "python3";
            if (OperatingSystem.IsWindows())
                pythonPath = "python3.exe";

            if (Processes.pythonOverridePath != null && Processes.pythonOverridePath.Length > 0) {
                if (Program.debug)
                    Console.WriteLine("Using custom python: '" + Processes.pythonOverridePath + "'");
                pythonPath = Processes.pythonOverridePath;
            }

            Process python = StartProcessWithOptions(pythonPath, arguments, workingDirectory);
            if (python == null || python.StandardError.ReadToEnd().Contains("not found")) {
                pythonPath = AppContext.BaseDirectory + "python/" + "python";
                if (OperatingSystem.IsWindows())
                    pythonPath = AppContext.BaseDirectory + "python\\" + "python.exe";
                python = StartProcessWithOptions(pythonPath, arguments, workingDirectory);
            }
            if (python == null) {
                throw new Exception("Cannot get Python running!");
            }
            return python;
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
