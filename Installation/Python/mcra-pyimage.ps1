# Creates a local install image for Python and required MCRA packages.
# Usage:
#
# PS> ./mcra-pyimage.ps1 -ImageFolder "c:\mcra-py"
#

param (
    [string]$ImageFolder = "$PsScriptRoot"
)

$PythonVersion = '3.12.4'
$PythonInstaller = "python-$PythonVersion-amd64.exe"

# Create target folder if not exists
If (!(test-path -PathType container $ImageFolder))
{
      New-Item -ItemType Directory -Path $ImageFolder
}
#Remove-Item "$ImageFolder\*" -Recurse -Force

# Download Python
Write-Host "Downloading Python $PythonVersion installer ..." -ForegroundColor green
Invoke-WebRequest -Uri "https://www.python.org/ftp/python/$PythonVersion/python-$PythonVersion-amd64.exe" -OutFile "$ImageFolder\python-$PythonVersion-amd64.exe"

# Create local installer layout, using the Python installer
Write-Host "Creating local installer layout ..." -ForegroundColor green
Start-Process -FilePath "$ImageFolder\$PythonInstaller" `
     -ArgumentList "/layout $ImageFolder", "/quiet" `
     -NoNewWindow `
     -Wait `
     -PassThru ` > $null

# Download pip packges
Write-Host "Downloading pip packages ..." -ForegroundColor green
New-Item -Name "pip" -Path "$ImageFolder" -ItemType Directory > $null

$jsonData = Get-Content -Path "$PsScriptRoot\mcra-pip-packages.json" | ConvertFrom-Json
$packagesArray = $jsonData.packages
foreach ($package in $packagesArray) {
    Invoke-WebRequest -Uri $($package.url) -OutFile "$ImageFolder\pip\$($package.file)"
}

# Copy install scripts
if ($ImageFolder -ne $PsScriptRoot) {
    Write-Host "Copy install scripts ..." -ForegroundColor green
    Copy-item -Path "$PsScriptRoot\mcra-pyinstall.ps1" -Destination "$ImageFolder" > $null
    Copy-item -Path "$PsScriptRoot\mcra-pip-packages.json" -Destination "$ImageFolder" > $null
}

Write-Host "Done!" -ForegroundColor green
