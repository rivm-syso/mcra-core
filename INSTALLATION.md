# Building MCRA.Core

## **Introduction**

These are the instructions for building MCRA.Core and the command-line (CLI) utility from source code. Refer to the [README.md](./README.md) file if you only want to run the CLI utility without building the utility from sources.

## **System requirements**

### Supported Operating System

Windows 10, Windows 11, Windows Server 2016, Windows Server 2019, Windows Server 2022


### Required software


The following software is needed to succesfully compile the software. Install the software with local administrator rights and with Powershell ExecutionPolicy as Unrestricted.

|Software                       |Version  |Purpose                                           |Download URLs                                                                      |Required? |
|-------------------------------|---------|--------------------------------------------------|-----------------------------------------------------------------------------------|----------|
|.NET SDK                       | 6.0     |Tools to build and run C# .NET applications       |[Website Microsoft](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)        |Required  |    
|R                              | R-4.2.1 |Statistical Analysis                              |[R Project site](https://cran.r-project.org/bin/windows/base/old/4.2.1/)           |Required  |
|RTools                         | 4.2     |To build R PBK model binaries                     |[RTools site](https://cran.r-project.org/bin/windows/Rtools/)                      |Required  |
|Access Database Engine         | 2016    |To read MS Access database files                  |[Micrsoft website](https://www.microsoft.com/en-us/download/details.aspx?id=54920) |Optional <sup>1</sup> |
|Git                            | 2.0 or later|To clone the MCRA Core repository for development |[Website Git](https://git-scm.com/)                                                |Optional <sup>2</sup>  |
|

<sup>1</sup> Only needed when MS Access MDB files are used for input of the CLI utility.\
<sup>2</sup> Only needed when you want to develop and contribute to the MCRA Core source code.

## **Installing prerequites**

### **Required software**

### .NET SDK

* Download .NET 6.0 SDK, version Windows x64, from [Microsoft website](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* Run the installer and accept all defaults

### R

* Download the R-4.2.1 installer from the [R Project site](https://cran.r-project.org/bin/windows/base/old/4.2.1/).
* Run the installer and accept all defaults.

### RTools

* Download RTools 4.2 installer from the [RTools site](https://cran.r-project.org/bin/windows/Rtools/); beware that this version of RTools is compatible with the installed version of R.
* Run the installer and accept all defaults. RTools will install directly on the system root in C:\rtools40.
* Add two folders to your Windows System Environment Path variable [(see also general Windows instructions)](https://www.wikihow.com/Change-the-PATH-Environment-Variable-on-Windows):
  * In the lower left Windows area, search for "path" 
  * Select Edit the System Environment Details
  * On the Advanced tab, click Environment Variables...
  * From the lower System Variables section, select Path, Edit, and add the following two directories:
    * C:\rtools40\mingw64\bin
    * C:\rtools40\usr\bin\
    <img src="./installation/media/EditEnvironmentVariablePath.jpg" alt="Edit environment image" height="200"/>

### **Optional software**

### MS Access Database Engine Redistributable (optional)

This component is only necessary if you plan to use MS Access MDF files as input of the CLI utility.
* Download the MS Access Database Engine Redistributable from de [Microsoft Website](https://www.microsoft.com/en-us/download/details.aspx?id=54920).
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
    git clone https://git.wur.nl/Biometris/MCRA.git -b mcra92
    ```
    * Dowload sources as ZIP file from https://git.wur.nl/Biometris/mcra-dev/MCRA/-/tree/mcra92/Mcra.Core.
* **Install additional R libraries**
  * In PowerShell console, running as Administrator, browse to folder .\Installation and run the command:
  ```
    .\InstallRPackages.ps1
  ```
* **Build sources**
  * Open PowerShell or a command prompt, browse to the root folder where you downloaded the sources, and run the command:
  ```
    dotnet build --configuration release mcra.core.sln
  ```
  This will build the release binaries, including the CLI tool. The CLI utility mcra.exe is located in folder C:\MCRA\Mcra.Core\MCRA.Simulation.Commander\bin\Release. For instructions on how to use the CLI utility, see [README.md](./README.md).
  
  
