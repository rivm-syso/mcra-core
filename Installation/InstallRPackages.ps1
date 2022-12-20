# Description   : this script installs additional R packages required for running MCRA Core.
# Creation date : 09-Dec-2022

# Definitions
$RVersion="4.2.1"
$Command = "$Env:Programfiles\R\R-$RVersion\bin\rscript.exe"
$LibTargetFolder=("$Env:Programfiles\R\R-$RVersion\library").Replace('\', '\\')
$PackageListFilePath = ".\rpackages.txt"
$ProastUrl = "https://www.rivm.nl/sites/default/files/2021-06/proast70.3.zip"

# Creates a unique temp folder to store the downloaded zip file
Function New-TemporaryFolder {
    # Make a new folder based upon a TempFileName
    $T="$($Env:temp)\tmp$([convert]::tostring((get-random 65535),16).padleft(4,'0')).tmp"
    New-Item -ItemType Directory -Path $T
}
$TempFolder = New-TemporaryFolder

# Exception: install the PROAST R package from a downloadeded zip file
Function InstallProastPackage {
    $DownloadZipFile = "$TempFolder\" + $(Split-Path -Path $ProastUrl -Leaf)
    Invoke-WebRequest -Uri $ProastUrl -OutFile $DownloadZipFile

    $DownloadZipFile = $DownloadZipFile.Replace('\', '\\')
    & "$Command" -e "install.packages('$DownloadZipFile', '$LibTargetFolder', repos=NULL, type='binary')"
}

# Install all packeges defined in 
$PackagesList = Get-Content -Path $PackageListFilePath
ForEach ($Package in $PackagesList)
{
    $Package = $Package.Trim()
    if ($Package.Contains("proast")) { 
        InstallProastPackage
      } 
      else {
        & "$Command" -e "install.packages('$Package', '$LibTargetFolder', repos='https://cloud.r-project.org', type='binary')"
      }
}
