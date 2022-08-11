using DoorDownloader;
using System.Diagnostics;
using System.Reflection;

internal class Program {
    private static void Main(string[] args) {
        //Get Python staged and ready to use before we pull a branch
        var DoorsDependenciesInstall = "-m pip install --user aenum fast-enum python-bps-continued colorama aioconsole websockets pyyaml";  //TODO The install script included seems to not play well with all this. Should investigate
        var PythonVersion = Processes.GetPyVersion();

        switch (PythonVersion) {
            case "3.10":
                //Nothing, we just need to allow it
                break;
            case "3.9":
                //DoorsDependenciesInstall = DoorsDependenciesInstall + " --py 3.9";
                break;
            case "3.8":
                //DoorsDependenciesInstall = DoorsDependenciesInstall + " --py 3.8";
                break;
            case "3.7":
                //DoorsDependenciesInstall = DoorsDependenciesInstall + " --py 3.7";
                break;
            case "3.6":
                //DoorsDependenciesInstall = DoorsDependenciesInstall + " --py 3.6";
                break;
            default:
                Web.FetchPython();
                break;
        }

        Console.WriteLine("DR Wrapper v" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " using Python v" + PythonVersion);
        List<DoorRepo> DoorsRepoList = DoorRepo.GetAll();
        Directory.CreateDirectory(AppContext.BaseDirectory + "GeneratedSeeds");

        Console.WriteLine("These are the currently supported branches: ");

        foreach (var Choice in DoorsRepoList) {
            Console.WriteLine("[" + DoorsRepoList.IndexOf(Choice) + "] " + Choice.ToString());
        }


        //Collect their choice, or quit
        Console.WriteLine("Enter the number corresponding to the branch you want to launch (q to quit):");
        var DRChoice = Console.ReadLine();

        int Index;
        while (!int.TryParse(DRChoice, out Index)) {
            if (DRChoice == "q")
                Environment.Exit(0);
            Console.WriteLine("Invalid Choice! Enter the number corresponding to the branch you want to launch (q to quit):");
            DRChoice = Console.ReadLine();
        }

        var RepositoryChoice = DoorsRepoList[Index];

        Console.WriteLine("Will attempt to clone/update the branch from " + RepositoryChoice.ToString() + "...");
        RepositoryChoice.PullRepo();

        setupDoorsDependencies(RepositoryChoice.LocalPath(), DoorsDependenciesInstall);

        Console.WriteLine("Launching the GUI for your selected branch. If nothing happens, there may be an error written below you can report!");
        launchDoorsGui(RepositoryChoice.LocalPath());

        //Methods
        static void launchDoorsGui(string directory) {
            Process doors = Processes.StartPythonWithOptions("./Gui.py --outputpath \"" + AppContext.BaseDirectory + "GeneratedSeeds\"", directory);
            Console.WriteLine(doors.StandardOutput.ReadToEnd());
            Console.WriteLine(doors.StandardError.ReadToEnd());
        }

        static void setupDoorsDependencies(string directory, string dependencies) {
            Process python = Processes.StartPythonWithOptions(dependencies, directory);
            Console.WriteLine(python.StandardOutput.ReadToEnd().Trim());
            Console.WriteLine(python.StandardError.ReadToEnd().Trim());
        }
    }
}