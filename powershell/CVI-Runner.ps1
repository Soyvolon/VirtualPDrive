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
$output = "D:\VirtualPDrive"
$local = ""
$noMods = $False

if($noMods) {
    & ".\Create-Virtual-Instance.ps1" $route $arma -nomods -output $output -local $local -Log -EnvVarName "VPD_VIRTUAL_INSTANCE_ID"
} else {
    $modsRaw = $mods -Join ", "
    & ".\Create-Virtual-Instance.ps1" $route $arma -mods $modsRaw -output $output -local $local -Log -EnvVarName "VPD_VIRTUAL_INSTANCE_ID"
}
