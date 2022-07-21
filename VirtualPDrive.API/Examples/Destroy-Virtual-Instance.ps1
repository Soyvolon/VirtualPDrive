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
    
    return
}

try {
    $res = Invoke-WebRequest -Uri "$Route/api/destroy/$idval" -Method "DELETE" -UseBasicParsing
}
catch {
    Write-Output "Bad request returned from server - no instance to destroy."
    return
}

$obj = $res | ConvertFrom-Json

Write-Output $obj
Write-Output $obj.success
