# DoorDownloader
A (mostly unnecessary) downloader and bootstrapper for the ALTTP Door Randomizer

### What does it do?

This program will automatically setup a self-contained Python environment, download (plus keep updated) a branch from Aerinon's Door Randomizer or Codemann8's Overworld Randomizer, and launch the GUI.

### Who is it for?

If you aren't familiar with Python and want to try out branches of the Door Randomizer besides the Stable branch, this script may help you to get running.

### What does it use?

This project utilizes the following projects, which I recommend you check out:
- [Aerinon's fork of the Door Randomizer](https://github.com/aerinon/ALttPDoorRandomizer)
- [Codemann8's fork of the Door Randomizer](https://github.com/codemann8/ALttPDoorRandomizer)
- [LibGit2Sharp](https://github.com/libgit2/libgit2sharp)
- [Miniconda](https://github.com/conda/conda)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [command-line-api](https://github.com/dotnet/command-line-api)
- [Eto.Forms](https://github.com/picoe/Eto)

## Setup
### Prerequisites
#### Windows:
- None

#### Mac:
- python-tk installed via brew

#### Linux:
- python3-tk installed via package manager

### Setup Instructions


#### Windows:
1. Download the zip for your platform (Most people probably want `win-x64.zip`)
2. Unzip it to a folder of its own as it will be creating files
3. Run DoorDownloader.exe

#### Mac:
1. Download `osx-x64.xip`
2. Unzip it to a folder of its own as it will be creating files (double clicking the zip should do this for you)
3. Right-click (2 finger click) on `libgit2-b7bad55.dylib` and select "Open". You will need to allow the binary in order to run it
4. Right-click (2 finger click) on `DoorDownloader` and select "Open". You will need to allow the binary in order to run it
5. Open Terminal and navigate to the folder you extracted the zip to
6. Run `chmod +x ./DoorDownloader`
7. Double-click DoorDownloader

#### Linux:
1. Download `linux-x64.xip`
2. Unzip it to a folder of its own as it will be creating files
3. Open your Terminal Emulator of choice and navigate to the folder you extracted the zip to
4. Run `chmod +x ./DoorDownloader`
5. Run ./DoorDownloader
