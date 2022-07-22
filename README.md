# VirtualPDrive

A Virtual file system designed to make building ArmA 3 mods requre signifigantly less hard drive space.

## Setup

### Console Runner

1. Get the latest runner from the [Releases](https://github.com/Soyvolon/VirtualPDrive/releases) page.
2. Enable the Windows Projected File System on your machine. Information about how to do that can be found [here](https://github.com/Microsoft/Windows-classic-samples/tree/main/Samples/ProjectedFileSystem)
3. Open up PowerShell (or another terminal of your choice) and run `VirtualPDrive.Runner.exe` from the command line. The arguments can be found below:

| Argument    | Description                     | Usage                                        |
| ----------- | ------------------------------- | -------------------------------------------- |
| ArmA 3 Path | The path to your ArmA 3 folder. | `VirtualPDrive.Runner.exe '/path/to/ArmA 3'` |

| Option | Long Option      | Description                                                             | Usage                                                                                      |
| ------ | ---------------- | ----------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| -m     | --mod            | Add a mod to the parameters. Specify none to load all workshop mods.    | `VirtualPDrive.Runner.exe -m @ACE -m @CBA -m @Another_Cool_Mod`                            |
| -o     | --output         | Output folder for ProjFS                                                | `VirtualPDrive.Runner.exe -o /arma3/pdrive`                                                |
|        | --no-mods        | Forces the application to only load base arma and local files.          | `VirtualPDrive.Runner.exe --no-mods`                                                       |
| -l     | --local          | A local directory to copy file names from into the virtual folder.      | `VirtualPDrive.Runner.exe --local /dummy_files`                                            |
| -e     | --extension      | Add an extension to the allowed file read whitelist.                    | `VirtualPDrive.Runner.exe -e .sqf -e .bin`                                                 |
|        | --load-whitelist | Add a file to the allowed file read whitelist.                          | `VirtualPDrive.Runner.exe --preload-whitelist config.bin --preload-whitelist myScript.sqf` |
| -p     | --preload        | Forces the runner to preload all whitelisted files before starting.     | `VirtualPDrive.Runner.exe --preload`                                                       |
|        | --no-clean       | Skips the step that cleans the output folder before starting.           | `VirtualPDrive.Runner.exe --no-clean`                                                      |
|        | --init-runners   | Set the number of concurrent file load operations are allowed to occur. | `VirtualPDrive.Runner.exe --init-runners 4`                                                |
|        | --no-purge       | Skips the step that cleans the output folder after ending.              | `VirtualPDrive.Runner.exe --no-purge`                                                      |

Example Call (PowerShell):
```ps
.\VirtualPDrive.Runner.exe "D:\SteamLibrary\steamapps\common\Arma 3" -o "output" -m "@212th Auxiliary Assets" -m "@3AS (Beta Test)" -m "@ace" -m "@CBA_A3" -m "@Last Force Project" -m "@Legion Studios Base - Stable" -m "@Just Like The Simulations - The Great War" -m "@Kobra Mod Pack - Main" -m "@Operation TREBUCHET" -m "@WebKnight Droids" -m "@327th Brokkrs Workshop" -m "@91st MRC - Auxilliary Mod" -m "@DBA CIS" -m "@DBA Core" -m "@DBA Republic" -l "dummy_files" --preload --preload-whitelist config.bin --init-runners 4
```

Example files are found in the `Examples` folder shipped with the Console Runner. You can also read about them [here](https://github.com/Soyvolon/VirtualPDrive/tree/development/VirtualPDrive.Runner/Examples)

### API

1. Get the latest API from the [Releases](https://github.com/Soyvolon/VirtualPDrive/releases) page.
2. Enable the Windows Projected File System on your machine. Information about how to do that can be found [here](https://github.com/Microsoft/Windows-classic-samples/tree/main/Samples/ProjectedFileSystem)
3. Run the API Application - its now listening on `http://localhost:9127`. You can change that in the `appsettings.json` file.
4. Make a request to the API - full documentation on the API can be found [here](https://documenter.getpostman.com/view/6009085/UzR1M3cc)

Example files are found in the `Examples` folder shipped with the API. You can also read about them [here](https://github.com/Soyvolon/VirtualPDrive/tree/development/VirtualPDrive.API/Examples)

# Credits

A big thanks to the people at [ProjFS-Managed-API](https://github.com/microsoft/ProjFS-Managed-API) who built the example file system provider that this is based off of. Furthermore, they maintain the `ProjectedFSLib.Managed.API` that this project utilizes to properly create a virtual environment. As such, make sure you check out their [LICENSE](https://github.com/microsoft/ProjFS-Managed-API/blob/main/LICENSE) as well!
