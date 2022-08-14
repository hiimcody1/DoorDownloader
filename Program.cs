using DoorDownloader;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reflection;
using System.CommandLine;

internal class Program {

    public static string version = "v" + Assembly.GetExecutingAssembly().GetName().Version.Major + "." + Assembly.GetExecutingAssembly().GetName().Version.Minor + "." + Assembly.GetExecutingAssembly().GetName().Version.Build + "-beta";
    public static bool debug = false;
    static async Task Main(string[] args) {
        var forceUpdate = new Option<bool>(
            name: "--alwaysupdaterepo",
            description: "Force updating of the selected repository",
            getDefaultValue: () => false
        );
        var forcedRepository = new Option<string>(
            name: "--repo",
            description: "Use a specific Git repository without selecting from a list"
        );
        var forcedBranch = new Option<string>(
            name: "--branch",
            description: "Use a specific repository branch instead of the default"
        );
        var forcedPython = new Option<string>(
            name: "--python",
            description: "Use a manually specified python installation",
            getDefaultValue: () => ""
        );
        var outputPath = new Option<string>(
            name: "--outputpath",
            description: "Where to put the outputted files from the Door Randomizer",
            getDefaultValue: () => AppContext.BaseDirectory + "GeneratedSeeds"
        );
        var romPath = new Option<FileInfo>(
            name: "--rom",
            description: "Path to your Zelda no Densetsu - Kamigami no Triforce ROM",
            getDefaultValue: () => new FileInfo(AppContext.BaseDirectory)
        );
        var createShortcut = new Option<bool>(
            name: "--createshortcut",
            description: "Create a shortcut to quickly launch the branch you select using the --repo and --branch options. This will also include the --output-path, --rom, and --force-update-repo arguments if they are specified",
            getDefaultValue: () => false
        );
        var debug = new Option<bool>(
            name: "--debug",
            description: "Enable debug output (Use this for reporting errors)"
        );
        var rootCommand = new RootCommand("Download and launch a branch of the ALttP Door Randomizer");
        rootCommand.Add(forceUpdate);
        rootCommand.Add(forcedRepository);
        rootCommand.Add(forcedBranch);
        //rootCommand.Add(forcedPython);
        rootCommand.Add(outputPath);
        rootCommand.Add(romPath);
        rootCommand.Add(createShortcut);
        rootCommand.Add(debug);

        rootCommand.SetHandler((forceUpdateValue, forcedRepositoryValue, forcedBranchValue, forcedPythonValue, outputPathValue, romPathValue, createShortcutValue, debugValue) => {
            //Arguments we need to pass to subclasses
            DoorRepo.forceUpdates = forceUpdateValue;
            Processes.pythonOverridePath = forcedPythonValue;
            Program.debug = debugValue;

            //Get Python staged and ready to use before we pull a branch
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
                    PythonVersion = Processes.GetPyVersion();
                    break;
            }

            Console.WriteLine("DR Wrapper " + Program.version + " using Python v" + PythonVersion);

            if (forcedRepositoryValue != null && forcedBranchValue != null) {
                Console.WriteLine("Auto launching a branch...");
                DoorRepoBranch forcedBranch = new DoorRepoBranch(forcedBranchValue, forcedBranchValue);
                forcedBranch.BaseRepoUrl = forcedRepositoryValue;
                if (DoorRepo.forceUpdates)
                    forcedBranch.Pull();
                launchDoorsGui(forcedBranch.BasePath);
                return;
            }

            try {
                var checkUpdates = fetchChoice("Do you want to check for program updates? Enter Y or N. (Default: N)");
                if (checkUpdates != null && checkUpdates.ToLower().StartsWith("y")) {
                    string latestVersion = Web.CheckForUpdates();
                    if (latestVersion != Program.version) {
                        Console.WriteLine("New version available: " + latestVersion);
                        Console.WriteLine("Download here: https://github.com/hiimcody1/DoorDownloader/releases/tag/" + latestVersion);
                        fetchChoice("Press enter to continue");
                    }
                }
            } catch (Exception) {
                if (Program.debug)
                    Console.WriteLine("Failed to check for updates");
            }

            List<DoorRepo> DoorsRepoList = JsonConvert.DeserializeObject<List<DoorRepo>>(File.ReadAllText(AppContext.BaseDirectory + "Repositories.json"));

            var BranchList = new List<DoorRepoBranch>();
            foreach (var doorRepo in DoorsRepoList) {
                doorRepo.Branches.ForEach(delegate (DoorRepoBranch branch) {
                    branch.BaseRepoOwner = doorRepo.Owner;
                    branch.BaseRepoUrl = doorRepo.BaseUrl;
                    BranchList.Add(branch);
                });
            }

            Directory.CreateDirectory(outputPathValue);

            Console.WriteLine("Available branches (Add more using Repositories.json)");
            foreach (var Choice in BranchList) {
                Console.WriteLine("[" + BranchList.IndexOf(Choice) + "] " + Choice.ToFriendlyString());
            }


            //Collect their choice, or quit
            var DRChoice = fetchChoice("Enter the number corresponding to the branch you want to launch (q to quit):");

            int Index;
            while (!int.TryParse(DRChoice, out Index)) {
                if (DRChoice == "q")
                    Environment.Exit(0);
                DRChoice = fetchChoice("Invalid Choice! Enter the number corresponding to the branch you want to launch (q to quit):");
            }

            var RepositoryChoice = BranchList[Index];

            Console.WriteLine("Will attempt to clone/update the branch from " + RepositoryChoice.ToString() + "...");
            RepositoryChoice.Pull();

            Console.WriteLine("Checking/Resolving dependencies for the gui...");
            setupDoorsDependencies(RepositoryChoice.BasePath);

            var wantsShortcut = fetchChoice("Do you want to create a shortcut to quickly launch this branch in the future? Enter Y or N. (Default: N)");
            if (wantsShortcut != null && wantsShortcut.ToLower().StartsWith("y"))
                createShortcutValue = true;

            if(createShortcutValue) {
                createShortcutForBranch(RepositoryChoice);
            } else {
                Console.WriteLine("Launching the GUI for your selected branch. If nothing happens, re-run with --debug and there may be an error listed below you can report!");
                launchDoorsGui(RepositoryChoice.BasePath);
            }

            //Methods
            void launchDoorsGui(string directory) {
                string rom = "";
                
                if (romPathValue.ToString() != AppContext.BaseDirectory)
                    rom = " --rom \"" + romPathValue + "\"";

                Process doors = Processes.StartPythonWithOptions("./Gui.py --outputpath \"" + outputPathValue + "\"" + rom, directory);
                if (Program.debug) {
                    Console.WriteLine(doors.StandardOutput.ReadToEnd());
                    Console.WriteLine(doors.StandardError.ReadToEnd());
                }
            }

            void setupDoorsDependencies(string directory) {
                Process python = Processes.StartPythonWithOptions("-m pip install --user -r "+directory+"/resources/app/meta/manifests/pip_requirements.txt", directory);
                if (Program.debug) {
                    Console.WriteLine(python.StandardOutput.ReadToEnd().Trim());
                    Console.WriteLine(python.StandardError.ReadToEnd().Trim());
                }
                python.WaitForExit();
            }

            void createShortcutForBranch(DoorRepoBranch branch) {
                string additionalArgs = "";

                if (romPathValue.ToString() != AppContext.BaseDirectory)
                    additionalArgs = " --rom \"" + romPathValue + "\"";

                if (DoorRepo.forceUpdates)
                    additionalArgs += " --alwaysupdaterepo";

                StreamWriter shortcut;
                if (System.OperatingSystem.IsWindows()) {
                    shortcut = File.CreateText(AppContext.BaseDirectory + branch.BaseRepoOwner + "-" + branch.FriendlyName + ".bat");
                } else {
                    shortcut = File.CreateText(AppContext.BaseDirectory + branch.BaseRepoOwner + "-" + branch.FriendlyName + ".sh");
                }
                shortcut.WriteLine("\"" + AppContext.BaseDirectory + System.AppDomain.CurrentDomain.FriendlyName + "\" --repo \"" + branch.BaseRepoUrl + "\"" + " --branch \"" + branch.BranchName + "\"" + " --outputpath \"" + outputPathValue + "\"" + additionalArgs);
                shortcut.Close();
                if (!System.OperatingSystem.IsWindows()) {
                    //Chmod it on Linux/Mac
                    Process chmod = Processes.StartProcessWithOptions("bash", "-c \"chmod +x " + AppContext.BaseDirectory + branch.BaseRepoOwner + "-" + branch.FriendlyName + ".sh" + "\"");
                    chmod.WaitForExit();
                }
            }

            string? fetchChoice(string promptMessage) {
                Console.WriteLine(promptMessage);
                return Console.ReadLine();
            }
        }, forceUpdate, forcedRepository, forcedBranch, forcedPython, outputPath, romPath, createShortcut, debug);

        await rootCommand.InvokeAsync(args);
    }
}