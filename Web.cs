using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DoorDownloader {
    internal class Web {

        public static string CheckForUpdates() {
            var httpClient = new HttpClient();
            var productInfo = new ProductInfoHeaderValue("DoorRandomizer-Downloader", Program.version);
            httpClient.DefaultRequestHeaders.UserAgent.Add(productInfo);
            var latestReleaseAsync = httpClient.GetStringAsync("https://api.github.com/repos/hiimcody1/DoorDownloader/releases/latest");
            latestReleaseAsync.Wait();
            JObject latestReleaseJson = JObject.Parse(latestReleaseAsync.Result);
            string latestRelease = latestReleaseJson.GetValue("tag_name").ToString();
            if (Program.debug) {
                Console.WriteLine("Latest Release: " + latestRelease);
                Console.WriteLine("  This Release: " + Program.version);
            }
            return latestRelease;
        }
        public static void FetchBrew() {

        }

        public static void FetchPython() {
            var httpClient = new HttpClient();
            string pythonToDL;
            string installer;
            string installerArgs;
            if (System.OperatingSystem.IsWindows()) {
                if (Environment.Is64BitOperatingSystem)
                    pythonToDL = "https://repo.anaconda.com/miniconda/Miniconda3-py38_4.9.2-Windows-x86_64.exe";
                else
                    pythonToDL = "https://repo.anaconda.com/miniconda/Miniconda3-py38_4.9.2-Windows-x86.exe";
                installer = ".\\instPython.exe";
                installerArgs = "/S /RegisterPython=0 /D=python";
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
                installer = "./instPython.sh";
                installerArgs = "-b -p /tmp/ddpython";
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
                Process chmod = Processes.StartProcessWithOptions("bash", "-c \"chmod +x \\\"" + AppContext.BaseDirectory + installer + "\\\"\"");
                chmod.WaitForExit();
            }

            Process process = Processes.StartProcessWithOptions(installer, installerArgs, AppContext.BaseDirectory);
            Console.WriteLine("Installing local python instance, if things freeze here longer than 2 minutes, something has gone wrong...");
            process.WaitForExit();
            try {
                if (System.OperatingSystem.IsLinux() || System.OperatingSystem.IsMacOS()) {
                    Process chmod = Processes.StartProcessWithOptions("bash", "-c \"mv /tmp/ddpython \\\"" + AppContext.BaseDirectory + "python\\\"\"");
                    chmod.WaitForExit();
                }
            } catch (Exception ex) {
                if(Program.debug)
                    Console.WriteLine(ex.ToString());
            }
            if (!File.Exists(AppContext.BaseDirectory + "python/DLLs/libcrypto-1_1-x64.dll") && System.OperatingSystem.IsWindows()) {
                //They need to be copied
                File.Copy(AppContext.BaseDirectory + "python/Library/bin/libcrypto-1_1-x64.dll", AppContext.BaseDirectory + "python/DLLs/libcrypto-1_1-x64.dll");
                File.Copy(AppContext.BaseDirectory + "python/Library/bin/libcrypto-1_1-x64.pdb", AppContext.BaseDirectory + "python/DLLs/libcrypto-1_1-x64.pdb");
                File.Copy(AppContext.BaseDirectory + "python/Library/bin/libssl-1_1-x64.dll", AppContext.BaseDirectory + "python/DLLs/libssl-1_1-x64.dll");
                File.Copy(AppContext.BaseDirectory + "python/Library/bin/libssl-1_1-x64.pdb", AppContext.BaseDirectory + "python/DLLs/libssl-1_1-x64.pdb");
            }
        }
    }
}
