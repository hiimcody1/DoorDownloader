using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorDownloader {
    internal class Web {
        public static void FetchBrew() {

        }

        public static void FetchPython() {
            var httpClient = new HttpClient();
            string pythonToDL;
            string installer;
            string installerArgs;
            if (System.OperatingSystem.IsWindows()) {
                if (Environment.Is64BitOperatingSystem)
                    pythonToDL = "https://repo.anaconda.com/miniconda/Miniconda3-latest-Windows-x86_64.exe";
                else
                    pythonToDL = "https://repo.anaconda.com/miniconda/Miniconda3-latest-Windows-x86.exe";
                installer = "instPython.exe";
                installerArgs = "/S /RegisterPython=0 /D=" + AppContext.BaseDirectory + "python";
            } else if (System.OperatingSystem.IsMacOS()) {
                //TODO We need to install brew for Mac since we need python-tk
                /*
                if (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64) //M1 Mac
                    pythonToDL = "https://repo.anaconda.com/miniconda/Miniconda3-latest-MacOSX-arm64.sh";
                else
                    pythonToDL = "https://repo.anaconda.com/miniconda/Miniconda3-latest-MacOSX-x86_64.sh";
                installer = "instPython.sh";
                installerArgs = "-b -p " + AppContext.BaseDirectory + "python";
                */
                throw new Exception("Unsupported platform! " + Environment.OSVersion);
            } else if (System.OperatingSystem.IsLinux()) {
                pythonToDL = "https://repo.anaconda.com/miniconda/Miniconda3-latest-Linux-x86_64.sh";
                installer = "instPython.sh";
                installerArgs = "-b -p " + AppContext.BaseDirectory + "python";
            } else {
                throw new Exception("Unsupported platform! " + Environment.OSVersion);
            }

            var stream = httpClient.GetStreamAsync(pythonToDL);

            var localFile = new FileStream(AppContext.BaseDirectory + installer, FileMode.OpenOrCreate);

            stream.Wait();
            stream.Result.CopyTo(localFile);
            localFile.Close();

            if (System.OperatingSystem.IsLinux() || System.OperatingSystem.IsMacOS()) {
                //Chmod it on Linux/Mac
                Process chmod = Processes.StartProcessWithOptions("bash", "-c \"chmod +x " + AppContext.BaseDirectory + installer + "\"");
                chmod.WaitForExit();
            }

            Process process = Processes.StartProcessWithOptions(AppContext.BaseDirectory + installer, installerArgs);
            Console.WriteLine("Installing local python instance, if things freeze here longer than 2 minutes, something has gone wrong...");
            process.WaitForExit();
        }
    }
}
