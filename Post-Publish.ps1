param (
    [string]$Prefix = "publish",
    [string]$Dir = ""
)

try {
    Write-Output "Starting compression of $Dir"

    $date = Get-Date -Format yyyy-MM-dd_hh-mm-ss
    $file = "$Prefix-$date.7z"

    Write-Output "Output file will be $file"

    $7z = "$((Get-ItemProperty "HKCU:\SOFTWARE\7-Zip").Path)\7z.exe"

    Write-Output $7z

    & "$7z" a $file $Dir

    Write-Output "Operation complete."
}
catch {
    exit 1
}