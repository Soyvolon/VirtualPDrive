# VirtualPDrive

A Virtual file system designed to make building ArmA 3 mods requre signifigantly less hard drive space.

## Setup

### Console Runner

1. Get the latest runner from the [Releases](https://github.com/Soyvolon/VirtualPDrive/releases) page.
2. Enable the Windows Projected File System on your machine. Information about how to do that can be found [here](https://github.com/Microsoft/Windows-classic-samples/tree/main/Samples/ProjectedFileSystem)
3. Open up PowerShell (or another terminal of your choice) and run `VirtualPDrive.Runner.exe` from the command line. The arguments can be found below:

| Argument    | Description                     | Usage                                 |
| ----------- | ------------------------------- | ------------------------------------- |
| ArmA 3 Path | The path to your ArmA 3 folder. | `VirtualPDrive.exe '/path/to/ArmA 3'` |

| Option | Long Option | Description                                                          | Usage                                                    |
| ------ | ----------- | -------------------------------------------------------------------- | -------------------------------------------------------- |
| -m     | --mod       | Add a mod to the parameters. Specify none to load all workshop mods. | `VirtualPDrive.exe -m @ACE -m @CBA -m @Another_Cool_Mod` |
| -o     | --output    | Output folder for ProjFS                                             | `VirtualPDrive.exe -o /arma3/pdrive`                     |
|        | --no-mods   | Forces the application to only load base arma and local files.       | `VirtualPDrive.exe --no-mods`                            |
| -l     | --local     | A local directory to copy file names from into the virtual folder.   | `VirtualPDrive.exe --local /dummy_files`                 |

Example Call (PowerShell):
```ps
.\VirtualPDrive.exe "D:\SteamLibrary\steamapps\common\Arma 3" -o "output" -m "@212th Auxiliary Assets" -m "@3AS (Beta Test)" -m "@ace" -m "@CBA_A3" -m "@Last Force Project" -m "@Legion Studios Base - Stable" -m "@Just Like The Simulations - The Great War" -m "@Kobra Mod Pack - Main" -m "@Operation TREBUCHET" -m "@WebKnight Droids" -m "@327th Brokkrs Workshop" -m "@91st MRC - Auxilliary Mod" -m "@DBA CIS" -m "@DBA Core" -m "@DBA Republic"
```

### API

1. Get the latest API from the [Releases](https://github.com/Soyvolon/VirtualPDrive/releases) page.
2. Enable the Windows Projected File System on your machine. Information about how to do that can be found [here](https://github.com/Microsoft/Windows-classic-samples/tree/main/Samples/ProjectedFileSystem)
3. Run the API Application - its now listening on `http://localhost:9127`. You can change that in the `appsettings.json` file.
4. Make a request to the API - full documentation on the API can be found [here](https://documenter.getpostman.com/view/6009085/UzR1Jh6E#be08d7f6-ed5b-4581-a84c-2e6e1450ade3)

Example files are found in the `Examples` folder shipped with the API. You can also read about them [here]()

# Credits

A big thanks to the people at [ProjFS-Managed-API](https://github.com/microsoft/ProjFS-Managed-API) who built the example file system provider that this is based off of. Furthermore, they maintain the `ProjectedFSLib.Managed.API` that this project utilizes to properly create a virtual environment. As such, make sure you check out their [LICENSE](https://github.com/microsoft/ProjFS-Managed-API/blob/main/LICENSE) as well!
