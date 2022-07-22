# Description

This folder contains some examples of using powershell to launch a single virtual instance. As this
is a single application that runs, there is no way to track events and it is up to the user to 
wait until the virtual drive is ready for use.

## `Launch-VPD.ps1`

This will create a new virtual instance. Instance logs are printed to `stdout`.

| Param      | Required | Content                                                                            | Example                                  |
| ---------- | -------- | ---------------------------------------------------------------------------------- | ---------------------------------------- |
| ExePath    | True     | Path to VirtualPDrive.Runner.exe.                                                  | `..\VirtualPDrive.Runner.exe`            |
| Arma       | True     | Path to ArmA 3.                                                                    | `/path/to/Arma3`                         |
| Mods       | False    | List of mods.                                                                      | `-Mods "@CBA", "@ACE"`                   |
| NoMods     | False    | Disables mods.                                                                     | `-NoMods`                                |
| Output     | False    | Output for the virtual file system                                                 | `-Output /output/path`                   |
| Local      | False    | Local files to add to the virtual file system.                                     | `-Local /local/files`                    |
| Extensions | False    | A list of file extensions to whitelist for file reading.                           | `-Extensions ".bin, .sqf"`               |
| Whitelist  | False    | A list of file names to whitelist for file reading.                                | `-Whitelist "config.bin, texHeaders.bin` |
| Preload    | False    | Set if you want the instance to preload whitelisted files before starting.         | `-Preload`                               |
| Runners    | False    | Set to change the amount of concurrent load operations can happen.                 | `-Preload`                               |
| No Clean   | False    | Setting this will prevent the app from cleaning the output folder before starting. | `-Noclean`                               |
| No Purge   | False    | Setting this will prevent the app from cleaning the output folder after ending.    | `-NoPurge`                               |
| Log        | False    | Enables powershell logging.                                                        | `-Log`                                   |

## `LVPD-Runner.ps1`

This file will launch `Launch-VPD.ps1` with the parameters configured in the file. It is designed to be an easier way of setting parameters than the command line.

| Param | Required | Content | Example |
| ----- | -------- | ------- | ------- |
