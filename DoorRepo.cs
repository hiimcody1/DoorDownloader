using LibGit2Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DoorDownloader {
    internal class DoorRepo {
        public string Owner { get; set; }
        public string BaseUrl { get; set; }
        public List<DoorRepoBranch> Branches { get; set; }

        public static bool forceUpdates = false;

        public DoorRepo(string owner, string baseUrl) {
            Owner = owner;
            BaseUrl = baseUrl;
            Branches = new List<DoorRepoBranch>();
        }

        [JsonConstructor]
        public DoorRepo(string owner, string baseUrl, List<DoorRepoBranch> branches) {
            Owner = owner;
            BaseUrl = baseUrl;
            Branches = branches;
        }

        public override string ToString() {
            return this.Owner;
        }
    }

    internal class DoorRepoBranch {
        public string BaseRepoUrl = "";
        public string BaseRepoOwner = "";
        public string BranchName { get; set; }
        public string FriendlyName { get; set; }
        public string BasePath;

        public bool ManualUpdateOnly = false;
        [JsonConstructor]
        public DoorRepoBranch(string branchName, string friendlyName) {
            BranchName = branchName;
            FriendlyName = friendlyName;
            BasePath = AppContext.BaseDirectory + branchName;
        }
        public DoorRepoBranch(string branchName, string friendlyName, bool manualUpdateOnly) {
            BranchName = branchName;
            FriendlyName = friendlyName;
            BasePath = AppContext.BaseDirectory + branchName;
            ManualUpdateOnly = true;
        }
        public override string ToString() {
            return this.BaseRepoUrl + this.BranchName;
        }

        public string ToFriendlyString() {
            return this.BaseRepoOwner + " - " + this.FriendlyName + " [" + LastCommit() + "]";
        }

        public void Pull() {
            Directory.CreateDirectory(this.BasePath);

            CloneOptions co = new CloneOptions();
            co.BranchName = this.BranchName;

            Repository doorRepo;

            try {
                doorRepo = new Repository(this.BasePath);
                if (this.ManualUpdateOnly && !DoorRepo.forceUpdates) {
                    Console.WriteLine("Skipping update for branch (marked as requiring manual update)");
                    return;
                }
            } catch (RepositoryNotFoundException) {
                Repository.Clone(this.BaseRepoUrl, this.BasePath, co);
                doorRepo = new Repository(this.BasePath);
            }

            Commands.Pull(doorRepo, new Signature("DoorsDownloader", "<>", new DateTimeOffset()), new PullOptions());
            //TODO create way to preserve flags in settings that don't exist for other branches
            /*
            try {
                //Sync settings across branches
                File.CreateSymbolicLink(this.basePath + "/resources/user/settings.json", AppContext.BaseDirectory + "settings.json");
            } catch (IOException) {
                //Silently fail
            }
            */
        }

        public DateTime? LastCommit() {
            //Only supported via GitHub for now as our Git library doesn't support shallow cloning and a full clone is expensive
            //Very easy to hit rate-limits using this, we probably need to re-evaluate how this is handled
            if(BaseRepoUrl.Contains("github.com/")) {
                try {
                    JObject? latestCommitData = Web.FetchArbritraryJsonFromUrl(BaseRepoUrl.Replace("https://github.com/", "https://api.github.com/repos/") + "branches/" + BranchName);
                    if (latestCommitData != null) {
                        var commit = latestCommitData.Children();
                        foreach(JToken tk in commit) {
                            Console.WriteLine(tk.ToString());
                        }
                        Console.WriteLine(commit.ToString());
                        /*
                        JToken committer = commit.SelectToken("committer");
                        JToken date = committer.SelectToken("date");
                        Console.WriteLine(commit);
                        Console.WriteLine(committer);
                        Console.WriteLine(date);
                        */
                    }
                } catch (Exception ex) {
                    if(Program.debug)
                        Console.WriteLine("Error while retrieving last commit date for branch: " + ex.ToString());
                }
            }
            return null;
        }
    }
}
