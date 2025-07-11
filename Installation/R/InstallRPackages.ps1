# Description   : this script installs additional R packages required for running MCRA Core.
# Creation date : 09-Dec-2022
# Modify date   : 15-Jul-2025

# Definitions
$RVersion="4.5.1"
$RLibraryVersion="4.5"
$RLibraryVersionProast="4.3"
$Command = "$Env:Programfiles\R\R-$RVersion\bin\rscript.exe"
$PackageListFilePath = ".\rpackages.txt"

# Update base packages in the new installation
& "$Command" -e "update.packages(ask=FALSE, type='win.binary', repos='https://cloud.r-project.org')"

# Install all packages defined in
$PackagesList = Get-Content -Path $PackageListFilePath
ForEach ($Package in $PackagesList)
{
  $Package = $Package.Trim()
  if ($Package.Contains("proast") -Or $Package.Contains("opex") -Or $Package.Contains("svglite")) {
    & "$Command" -e "install.packages('$Package', contriburl='https://biometris.github.io/MCRARpackages/bin/windows/contrib/$RLibraryVersionProast', type='win.binary')"
  }
  else {
    & "$Command" -e "install.packages('$Package', contriburl='https://biometris.github.io/MCRARpackages/bin/windows/contrib/$RLibraryVersion', type='win.binary')"
  }
}
