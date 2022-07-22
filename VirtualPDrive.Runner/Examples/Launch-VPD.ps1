param(
    [string]$ExePath,

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
    [switch]$Log = $false
)

if($Log) {
    Write-Output "Running with params:"
    Write-Output "Mods: $mods" 
    Write-Output "No Mods: $nomods"
    Write-Output "Output: $output"
    Write-Output "Local: $local"
    Write-Output "Extensions: $Extensions"
    Write-Output "Whitelist: $Whitelist"
    Write-Output "Preload: $Preload"
    Write-Output "Runners: $Runners"
    Write-Output "No Clean: $Noclean"
}

try {
    $params = @(
        $arma
        "--init-runners",
        $Runners
    )

    if($output -ne "") {
        $params += "--output"
        $params += $output
    }

    if ($local -ne "") {
        $params += "--local"
        $params += $local
    }

    foreach($item in $mods)
    {
        $params += "--mod"
        $params += $item
    }
    
    if ($NoMods) {
        $params += "--no-mods"
    }

    foreach($item in $Extensions)
    {
        $params += "--extension"
        $params += $item
    }

    foreach($item in $Whitelist)
    {
        $params += "--preload-whitelist"
        $params += $item
    }

    if ($Noclean) {
        $params += "--no-clean"
    }

    if ($Preload) {
        $params += "--preload"
    }

    if ($Log) {
        Write-Output $params
    }

    & $ExePath $params

    exit 0
}
catch {
    Write-Error $_
    exit 1
}