# Description

This folder contains some example powershell scripts for using the API, specifically intended for powershell based CI workflows.

## `Create-Virtual-Instance.ps1` 
This will create a new virtual instance and wait for the instance to be ready before exiting. It supports saving the ID of the instance to an env variable, and also outputs it to `stdout` as the last line.

| Param      | Required | Content                                        | Example                    |
| ---------- | -------- | ---------------------------------------------- | -------------------------- |
| Route      | True     | Route to API                                   | `http://localhost:9127`    |
| Arma       | True     | Path to ArmA 3                                 | `/path/to/Arma3`           |
| Mods       | False    | List of mods                                   | `-Mods "@CBA", "@ACE"`     |
| NoMods     | False    | Disables mods                                  | `-NoMods`                  |
| Output     | False    | Output for the virtual file system             | `-Output /output/path`     |
| Local      | False    | Local files to add to the virtual file system  | `-Local /local/files`      |
| Log        | False    | Enables powershell logging                     | `-Log`                     |
| EnvVarName | False    | Name of the env var to save the instance ID to | `-EnvVarName ENV_VAR_NAME` |

## `CVI-Runner.ps1` 
This script handles arguments for `Create-Virtual-Instance`. It makes it easier than typing it all out on the command line (and for repeated testing). This script will save use the env variable `VPD_VIRTUAL_INSTANCE_ID` when running create virtual instance, but feel free to edit the script to change that.

| Param | Required | Content | Example |
| ----- | -------- | ------- | ------- |

No Params - all configured within the file itself.

## `Destroy-Virtual-Instance.ps1`
This sends a request to the API to destroy the instance that was created with create virtual instance. It requires an ID to be passed to it, either with `-Id <id>` or `-EnvVarName <env var>`.

| Param      | Required | Content                                         | Example                                |
| ---------- | -------- | ----------------------------------------------- | -------------------------------------- |
| Route      | True     | Route to API                                    | `http://localhost:9127`                |
| Id         | False    | The ID of the instance to destroy               | `3249aaa8-c890-4e69-9055-e755fbd18a40` |
| EnvVarName | False    | Name of the env var to get the instance ID from | `-EnvVarName ENV_VAR_NAME`             |