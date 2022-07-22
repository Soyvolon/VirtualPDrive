# This file calls the Create-Virtual-Instance script. Its
# intent is to make it simpler than trying to write all the
# parameters in the command line.

$mods = @(
    "@212th Auxiliary Assets",
    "@3AS (Beta Test)",
    "@ace",
    "@CBA_A3",
    "@Last Force Project",
    "@Legion Studios Base - Stable",
    "@Just Like The Simulations - The Great War",
    "@Kobra Mod Pack - Main",
    "@Operation TREBUCHET",
    "@WebKnight Droids",
    "@327th Brokkrs Workshop",
    "@91st MRC - Auxilliary Mod",
    "@DBA CIS",
    "@DBA Core",
    "@DBA Republic"
)

$route = "http://localhost:9127"
$arma = "D:\SteamLibrary\steamapps\common\Arma 3"

$noMods = $False

$output = "D:\VirtualPDrive"
$local = ""

$extensions = @(
    ".bin"
)
$whitelist = @(
    "config.bin"
)
$Preload = $True
$initRunners = 2
$noClean = $False
$randomOutput = $False
$noPurge = $False

$params = @{
    "Route"=$route;
    "Arma"=$Arma;
    "Mods" = $Mods;
    "NoMods" = $NoMods;
    "Output" = $output;
    "Local" = $Local;
    "Extensions" = $Extensions;
    "Whitelist" = $Whitelist;
    "Preload" = $Preload;
    "Runners" = $initRunners;
    "Noclean" = $Noclean;
    "RandomOutput" = $randomOutput;
    "NoPurge" = $noPurge;
    "Log"=$true;
    "EnvVarName"="VPD_VIRTUAL_INSTANCE_ID";
}

Write-Output $params

& ".\Create-Virtual-Instance.ps1" @params
