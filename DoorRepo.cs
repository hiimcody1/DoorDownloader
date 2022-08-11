using LibGit2Sharp;

namespace DoorDownloader {
    internal class DoorRepo {
        protected string repoOwner;
        protected string baseURL;
        protected string friendlyName;
        protected string branchName;
        protected string basePath;

        public DoorRepo(string repoOwner, string baseURL, string friendlyName, string branchName) {
            this.repoOwner = repoOwner;
            this.baseURL = baseURL;
            this.friendlyName = friendlyName;
            this.branchName = branchName;
            this.basePath = AppContext.BaseDirectory + this.branchName;
        }

        public string URL() {
            return this.baseURL;
        }

        public string LocalPath() {
            return this.basePath;
        }

        public string FriendlyName() {
            return this.friendlyName;
        }

        public override string ToString() {
            return this.repoOwner + " - " + this.friendlyName;
        }

        public void PullRepo() {
            Directory.CreateDirectory(this.basePath);

            CloneOptions co = new CloneOptions();
            co.BranchName = this.branchName;

            Repository DoorRepo;

            try {
                DoorRepo = new Repository(this.basePath);
            } catch (RepositoryNotFoundException) {
                Repository.Clone(this.baseURL, this.basePath, co);
                DoorRepo = new Repository(this.basePath);
            }

            Commands.Pull(DoorRepo, new Signature("DoorsDownloader", "<>", new DateTimeOffset()), new PullOptions());
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

        public static List<DoorRepo> GetAll() {
            return new List<DoorRepo> {
                new DoorRepo("Aerinon", "https://github.com/aerinon/ALttPDoorRandomizer/", "Stable", "DoorDev"),
                new DoorRepo("Aerinon", "https://github.com/aerinon/ALttPDoorRandomizer/", "Unstable", "DoorDevUnstable"),
                new DoorRepo("Aerinon", "https://github.com/aerinon/ALttPDoorRandomizer/", "Volatile", "DoorDevVolatile"),
                new DoorRepo("Aerinon", "https://github.com/aerinon/ALttPDoorRandomizer/", "Customizer", "Customizer"),

                new DoorRepo("Codemann8", "https://github.com/codemann8/ALttPDoorRandomizer/", "OverworldShuffle", "OverworldShuffle"),
                new DoorRepo("Codemann8", "https://github.com/codemann8/ALttPDoorRandomizer/", "OverworldShuffleDev", "OverworldShuffleDev")
            };
        }
    }
}
