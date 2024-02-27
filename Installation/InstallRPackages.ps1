# Description   : this script installs additional R packages required for running MCRA Core.
# Creation date : 09-Dec-2022
# Modify date   : 01-Feb-2023

# Definitions
$RVersion="4.2.1"
$RVersionMajorMinor="4.2"
$RVersionMajorMinorProast="4.0"
$Command = "$Env:Programfiles\R\R-$RVersion\bin\rscript.exe"
$PackageListFilePath = ".\rpackages.txt"

# Update base packages in the new installation
& "$Command" -e "update.packages(ask=FALSE, type='win.binary', repos='https://cloud.r-project.org')"

# Install all packages defined in
$PackagesList = Get-Content -Path $PackageListFilePath
ForEach ($Package in $PackagesList)
{
  $Package = $Package.Trim()
  if ($Package.Contains("proast")) {
    & "$Command" -e "install.packages('$Package', contriburl='https://biometris.github.io/MCRARpackages/bin/windows/contrib/$RVersionMajorMinorProast', type='win.binary')"
  }
  else {
    & "$Command" -e "install.packages('$Package', contriburl='https://biometris.github.io/MCRARpackages/bin/windows/contrib/$RVersionMajorMinor', type='win.binary')"
  }
}
