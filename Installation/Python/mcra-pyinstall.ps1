# Installs Python and required MCRA packages from a local layout that was created
# with script mcra-pyimage.ps1. This installation does not require an internet connection.
# Usage: just run the script in the layout folder created by mcra-pyimage.ps1:
#
# PS> ./mcra-pyinstall.ps1
#

$PythonVersion = '3.12.4'
$PythonInstaller = "python-$PythonVersion-amd64.exe"
$PythonFolderVersion = $PythonVersion -replace '^(\d+)\.(\d+)\.(\d+)','$1$2'
$PythonTargetDir = "C:\Python$PythonFolderVersion"
$PipPackages = "$PsScriptRoot\mcra-pip-packages.json"

# Install Python
Write-Host "Installing Python $PythonVersion in folder $PythonTargetDir ..." -ForegroundColor green
Start-Process -FilePath "$PsScriptRoot\$PythonInstaller" `
    -ArgumentList "/quiet", "InstallAllUsers=1", "PrependPath=1", "TargetDir=$PythonTargetDir" `
    -NoNewWindow `
    -Wait `
    -PassThru ` > $null

# Add Python to the local path environment of the context in which this script is running
$addPath = $PythonTargetDir
$addPathScript = "$PythonTargetDir\Scripts"
$arrPath = $env:Path -split ';'
$env:Path = ($arrPath + $addPath + $addPathScript) -join ';'

# Add MCRA pip packages
Write-Host "Installing pip packges ..." -ForegroundColor green
$jsonData = Get-Content -Path "$PipPackages" | ConvertFrom-Json
$packagesArray = $jsonData.packages
foreach ($package in $packagesArray) {
    $file = $($package.file).Trim()
    $file = "$PsScriptRoot\pip\$file" 
    &"pip" install $file --no-deps --disable-pip-version-check
}
