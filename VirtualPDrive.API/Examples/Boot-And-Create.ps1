# This file is deisnged to launch the API
# as a separate application, saving the handle,
# and then running the CVI-Runner.ps1 file to spool
# up an instance.

param(
    [string]$ExePath
)

$cur = Get-Location

$path = Resolve-Path -Path $ExePath
$working = Split-Path -Path $path -Parent

Set-Location -Path $working

& "cmd" /c start $path

Start-Sleep -Seconds 1

Set-Location $cur

& ".\CVI-Runner.ps1"
