# This file is designed to communicate with an already existing
# Virtual P Drive API application and spool up a new instance.
# Keeping in mind CI, this script exits on error or after the
# instance is fully loaded.

param(
    [string]$Route,

    [string]$Arma,

    [Parameter(Mandatory=$False)]
    [string[]]$Mods,

    [Parameter(Mandatory=$False)]
    [switch]$NoMods = $false,

    [Parameter(Mandatory=$False)]
    [string]$Output = 'output',

    [Parameter(Mandatory=$False)]
    [string]$Local = "",

    [Parameter(Mandatory=$False)]
    [string[]]$Extensions,

    [Parameter(Mandatory=$False)]
    [string[]]$Whitelist,

    [Parameter(Mandatory=$False)]
    [switch]$Preload,

    [Parameter(Mandatory=$False)]
    [int32]$Runners = 2,

    [Parameter(Mandatory=$False)]
    [switch]$Noclean = $false,

    [Parameter(Mandatory=$False)]
    [switch]$RandomOutput = $false,

    [Parameter(Mandatory=$False)]
    [switch]$Log = $false,

    [Parameter(Mandatory=$False)]
    [string]$EnvVarName = ""
)

if($Log) {
    Write-Output "Running with params:"
    Write-Output "Route: $route"
    Write-Output "Mods: $mods" 
    Write-Output "No Mods: $nomods"
    Write-Output "Output: $output"
    Write-Output "Local: $local"
    Write-Output "Extensions: $Extensions"
    Write-Output "Whitelist: $Whitelist"
    Write-Output "Preload: $Preload"
    Write-Output "Runners: $Runners"
    Write-Output "No Clean: $Noclean"
    Write-Output "Random Output: $RandomOutput"
}

$body = @"
{
    "arma": "$($arma.Replace("\", "\\"))",
    "mods": ["$(($mods -Split ", ") -Join '", "')"],
    "noMods": $($noMods.ToString().ToLower()),
    "output": "$($output.Replace("\", "\\"))",
    "local": "$($local.Replace("\", "\\"))",
    "extensions": ["$(($Extensions -Split ", ") -Join '", "')"],
    "whitelist": ["$(($Whitelist -Split ", ") -Join '", "')"],
    "preLoad": $($Preload.ToString().ToLower()),
    "initRunners": $Runners,
    "noClean": $($Noclean.ToString().ToLower()),
    "randomOutput": $($RandomOutput.ToString().ToLower())
}
"@

if($Log) {
    Write-Output $body
}
try {
    $res = Invoke-WebRequest -Uri "$route/api/create" -ContentType "application/json" -Body $body -Method "POST" -UseBasicParsing
    $id = ($res | ConvertFrom-Json).instanceId

    if($Log) {
        Write-Output $res
        Write-Output $id
    }

    while ($true) {
        try {
            $checkres = Invoke-WebRequest -Uri "$route/api/instance/$id" -Method "GET" -UseBasicParsing
            $obj = $checkres | ConvertFrom-Json

            if($Log) {
                Write-Output $checkres
                Write-Output $obj
            }

            if ($obj.errored -or $obj.stopped) {
                
                Write-Error "Instance stopped/errored: $($obj.messages)"

                return
            }

            if($EnvVarName -ne "") {
                Set-Item "Env:$EnvVarName" $id
            }

            if($obj.loaded) {
                break
            } else {
                Start-Sleep -Seconds 5
            }
        } catch {
            Write-Error $_
            exit 1
        }
    }

    Write-Output "Virtual Instance ready."
    Write-Output $id

    if($EnvVarName -ne "") {
        Set-Item "Env:$EnvVarName" $id
    }

    exit 0
}
catch {
    Write-Error $_
    exit 1
}