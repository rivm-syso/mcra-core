# Building MCRA Core

## **Introduction**

These are the instructions for building MCRA Core and the command-line (CLI) utility from source code. Refer to the [README.md](./README.md) file if you only want to run the CLI utility without building the utility from sources.

## **System requirements**

### Supported Operating System

Windows 10, Windows 11, Windows Server 2016, Windows Server 2019, Windows Server 2022

### Required software

The following software is needed to successfully compile the software. Install the software with local administrator rights and with Powershell ExecutionPolicy as Unrestricted.

|Software                   |Version      |Purpose                                           |Download URLs                                                                      |Required?   |
|---------------------------|-------------|--------------------------------------------------|-----------------------------------------------------------------------------------|------------|
|.NET SDK                   | 6.0         |Tools to build and run C# .NET applications       |[Website Microsoft](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)        |Required    |
|R                          | R-4.2.1     |Statistical Analysis                              |[R Project site](https://cran.r-project.org/bin/windows/base/old/4.2.1/)           |Required    |
|RTools                     | 4.2         |To build R PBK model binaries                     |[RTools site](https://cran.r-project.org/bin/windows/Rtools/)                      |Required    |
|Python (and libRoadRunner)                    | 3.11        |To run PBK models based on SBML                   |[Python downloads site](https://www.python.org/downloads/)                         |Required    |
|Access Database Engine     | 2010        |To read MS Access database files                  |[Microsoft website](https://www.microsoft.com/en-us/download/details.aspx?id=13255)|Optional <sup>1</sup>|
|Git                        | 2.0 or later|To clone the MCRA Core repository for development |[Website Git](https://git-scm.com/)                                                |Optional <sup>2</sup>|
|

<sup>1</sup> Only needed when MS Access MDB files are used for input of the CLI utility.\
<sup>2</sup> Only needed when you want to develop and contribute to the MCRA Core source code.

## **Installing prerequites**

### **Required software**

### .NET SDK

* Download .NET 6.0 SDK, version Windows x64, from [Microsoft website](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).
* Run the installer and accept all defaults.

### R

* Download the R-4.2.1 installer from the [R Project site](https://cran.r-project.org/bin/windows/base/old/4.2.1/).
* Run the installer and accept all defaults.

### RTools

* Download RTools 4.2 installer from the [RTools site](https://cran.r-project.org/bin/windows/Rtools/); beware that this version of RTools is compatible with the installed version of R.
* Run the installer and accept all defaults. RTools will install directly on the system root in C:\rtools42.
* Add two folders to your Windows System Environment Path variable [(see also general Windows instructions)](https://www.wikihow.com/Change-the-PATH-Environment-Variable-on-Windows):
  * In the lower left Windows area, search for "path"
  * Select Edit the System Environment Details
  * On the Advanced tab, click Environment Variables...
  * From the lower System Variables section, select Path, Edit, and add the following two directories:
    * C:\rtools42\mingw64\bin
    * C:\rtools42\usr\bin\
    <img src="./installation/media/EditEnvironmentVariablePath.jpg" alt="Edit environment image" height="200"/>
  * It may be needed to restart your computer

### Python and libRoadRunner

* Download Python version 3.11.x, Windows installer (64-bit), from the [Python download site](https://www.python.org/downloads/).
* Run the installer:
  * Welcome screen, select:
    * Add python.exe to PATH
    * Customize installation
  * Optional Features, use all defaults, select Next.
  * Advanced Options:
    * Check Install Python 3.11 for all users
    * Customize install location to:

      ```C:\Python311```
  
  Select Install to start the installation.

* Install libRoadRunner 2.6.0. This Python package is required to run physiologically based kinetic (PBK) models in MCRA. Open a PowerShell console, and run the command:
  * pip install libroadrunner==2.6.0

### **Optional software**

### MS Access Database Engine 2010 Redistributable (optional)

This component is only necessary if you plan to use MS Access MDF files as input of the CLI utility.

* Download the MS Access Database Engine 2010 Redistributable from de [Microsoft Website](https://www.microsoft.com/en-us/download/details.aspx?id=13255).\
Note: a later version, for example 2016, may work but has not been tested. A known problem of the 2026 version is that the installation is aborted when a conflict is detected from mixing x86 and x64 Microsoft Office components.
* Run the installer and accept all defaults.

### Git (optional)

This component is only necessary if you plan to develop with MCRA Core and want version control.
* Get the latest version of [Git](https://git-scm.com/downloads)
* Run the installer and accept all defaults.


## Building the Code

* **Download Source Code**
    * Create a local folder for the sources, e.g. C:\MCRA
    * Using Git: in console window:
    ```
    git clone https://github.com/rivm-syso/mcra-core.git -b dev
    ```
    * Dowload sources as ZIP file from https://github.com/rivm-syso/mcra-core/archive/refs/heads/dev.zip.
* **Install additional R libraries**
  * In PowerShell console, running as Administrator, browse to folder .\Installation and run the command:
  ```
    .\InstallRPackages.ps1
  ```
* **Build sources**
  * Open PowerShell or a command prompt, browse to the root folder where you downloaded the sources, and run the command:
  ```
    dotnet build --configuration release mcra-core.sln
  ```
  This will build the release binaries, including the CLI tool. The CLI utility mcra.exe is located in folder C:\MCRA\mcra-core\MCRA.Simulation.Commander\bin\Release. For instructions on how to use the CLI utility, see [README.md](./README.md).
  
  
