# This file is deisnged to launch the API
# as a separate application, saving the handle,
# and then running the CVI-Runner.ps1 file to spool
# up an instance. This script also ensures
# the Windows Projected File system is enabled.

param(
    [string]$ExePath
)

$fres = Get-WmiObject -query "select * from Win32_OptionalFeature where name = 'Client-ProjFS' and installstate = 1"

if ($null -eq $fres) {
    Write-Output "Enabling ProjFS"

    if (-Not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')) 
    {
        $CommandLine = "-File `"" + $MyInvocation.MyCommand.Path + "`" " + $MyInvocation.UnboundArguments
        Start-Process -Wait -FilePath PowerShell.exe -Verb Runas -ArgumentList $CommandLine
    } else {
        Enable-WindowsOptionalFeature -Online -FeatureName Client-ProjFS -NoRestart
        Write-Output "Enabled ProjFS. This this again to boot the API."
        exit 0
    }
}

$cur = Get-Location
$path = Resolve-Path -Path $ExePath
$working = Split-Path -Path $path -Parent

Set-Location -Path $working

& "cmd" /c start $path

Start-Sleep -Seconds 1

Set-Location $cur

& ".\CVI-Runner.ps1"
