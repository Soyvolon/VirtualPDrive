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

# On publish
# $ExePath = "..\VirtualPDrive.Runner.exe"

# In Source
$ExePath = "..\bin\Debug\net6.0\VirtualPDrive.Runner.exe"

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
$noClean = $False
$noPurge = $False
$initRunners = 2

$params = @{
    "ExePath"=$ExePath;
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
    "NoPurge" = $noPurge;
    "Log"=$true;
}

Write-Output $params

& ".\Launch-VPD.ps1" @params
