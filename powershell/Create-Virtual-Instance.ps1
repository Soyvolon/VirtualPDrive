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
}

$body = @"
{
    "arma": "$($arma.Replace("\", "\\"))",
    "mods": ["$(($mods -Split ", ") -Join '", "')"],
    "noMods": $($noMods.ToString().ToLower()),
    "output": "$($output.Replace("\", "\\"))",
    "local": "$($local.Replace("\", "\\"))"
}
"@

if($Log) {
    Write-Output $body
}

$res = Invoke-WebRequest -Uri "$route/api/create" -ContentType "application/json" -Body $body -Method "POST" -UseBasicParsing
$id = ($res | ConvertFrom-Json).instanceId

if($Log) {
    Write-Output $res
    Write-Output $id
}

while ($true) {
    $checkres = Invoke-WebRequest -Uri "$route/api/instance/$id" -Method "GET" -UseBasicParsing
    $obj = $checkres | ConvertFrom-Json

    if($Log) {
        Write-Output $checkres
        Write-Output $obj
    }

    if ($obj.errored -or $obj.stopped) {
        
        Write-Error "Instance stopped/errored: $($obj.messages)"

        if($EnvVarName -ne "") {
            Set-Item "Env:$EnvVarName" $id
        }

        return
    }

    if($obj.loaded) {
        break
    } else {
        Start-Sleep -Seconds 2
    }
}

Write-Output "Virtual Instance ready."
Write-Output $id

if($EnvVarName -ne "") {
    Set-Item "Env:$EnvVarName" $id
}
