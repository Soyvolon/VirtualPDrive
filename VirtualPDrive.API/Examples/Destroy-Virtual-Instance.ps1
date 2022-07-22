# This script will request the API to destory an existing
# instance that matches the provided ID.

param(
    [string]$Route = "",

    [Parameter(Mandatory=$False)]
    [string]$Id = "",

    [Parameter(Mandatory=$False)]
    [string]$EnvVarName = ""
)

$idval = $id
if ($EnvVarName -ne "") {
    $idval = (Get-Item "Env:$EnvVarName").Value
}

if ($idval -eq "") {
    Write-Error "No ID was provided to request destruction for."
    
    exit 1
}

try {
    $res = Invoke-WebRequest -Uri "$Route/api/destroy/$idval" -Method "DELETE" -UseBasicParsing
}
catch {
    Write-Output $_
    exit 1
}

$obj = $res | ConvertFrom-Json

Write-Output $obj
Write-Output $obj.success

exit 0
