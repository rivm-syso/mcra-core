<img src="./Installation/Media/MCRAlogo.png" alt="MCRA logo" height="80"/>

# **MCRA Core**

MCRA Core is an open source collection of models in the Monte Carlo Risk Assessment (MCRA) platform. These models can be run as actions independently using a command-line interrface (CLI) or, more conveniently, by using the MCRA web application of which the most recent version can be found at https://mcra.rivm.nl. The core contains all the modules that are available in the MCRA web application, like for example methods to calculate exposures, hazards and risk assessment. For a complete overview of all available modules see the [modular design documentation](https://mcra.rivm.nl/documentation).  

MCRA Core comes with a command-line (CLI) utility *mcra.exe*, to run a selection of modules for your own study. You can either download the binaries or build the library and the CLI utility from the source files.

## **Table of Contents**

* [Features](#features)
* [Supported platforms](#Supported-platforms)
* [Running models with CLI utility](#CLI-utility)
* [Building from source code](#Build-from-source-code)
* [User documentation](#User-documentation)
* [Copyright](#Copyright)


## **Features**

* Source code available for all modules of the MCRA web application.
* CLI utility to run an action.
* The output results are provided as data files in CSV format for further processing, and in addition an HTML overview page with all the images in SVG format.


## **Supported platforms**

MCRA Core is available for 64-bit Windows.
### Supported Operating System

Windows 10, Windows 11, Windows Server 2016, Windows Server 2019, Windows Server 2022

## **Running the CLI utility**

The current release of the CLI utility of MCRA Core is available as download from Github (https://github.com/mcra/releases) or can be [built from the sources](#Build-from-source-code).

The most basic usage is to specify a zip file for the action that you want to compute. In a terminal window, e.g. PowerShell or the command prompt, run the command:
```
  mcra.exe run '<name_of_action>.zip' --dbType csv
```
The command line option dbType specifies that the input data files in the zip file are in CSV file format. For other possible command-line options, use the help command:

```
  mcra.exe help run
```
This will output a result similar to (exact details maybe different, depending on the version of the CLi utility),
```
  -o, --output                Output folder.
  --overwrite                 (Default: false) Overwrite existing output.
  --skipreport                (Default: false) Don't render report.
  --skiptables                (Default: false) Don't generate tables.
  --skipcharts                (Default: false) Don't generate charts.
  --keeptempfiles             (Default: false) Keep temp files.
  -r, --randomseed            Use this value as the Monte Carlo random seed for the project.
  -i, --interactive           (Default: false) Set to run in interactive mode.
  -s, --silent                (Default: false) Set to run in silent mode.
  --dbType                    (Default: Csv) Database type.
  --help                      Display this help screen.
  --version                   Display version information.
  Task input file (pos. 0)    Required. Input file containing the simulation task to be processed.
```

*Output*\
The CLI utility creates the output files in a subfolder with the name of the zip file, in the *.\Outputs* folder in the location where the CLI command is run.


*Example*

In this example, an action as zip file has been composed that defines a concentration model with focal measurement replacement of clothianidin on potatoes and a prospective occurrence percentage of 40%. This action can be run by the command:


```
  mcra.exe run 'Val-Single Value Risk adjustment.zip' --dbType csv
```

The full HTML report is located in folder *.\Outputs\Val-Single Value Risk adjustment\Out-Release-<data>-<run_id>\Report\_Report.html*. It contains all information on action inputs, the results for the sub-actions, and for the main single value risk action, as tables and images. The associated CSV data files are in the same subfolder.


## **Build from source code**

Instructions on how to build the CLI utility from the source code are described in the [Installation.md](./INSTALLATION.md) file.

## **User documentation** ##

A full description of all the modules and their relations can be found online at https://mcra.rivm.nl/documentation. Here you can find information on the input, the calculation settings, and the results for each module. For many modules, a theoretical foundation is provided to corroborate the MCRA Core implemention of the calculations.

## **Copyright**
MCRA Core is developed by Wageningen University & Research, Biometris for RIVM (2023)\
Copyright © 2023 [RIVM](https://www.rivm.nl/en/food-safety/chemicals-in-food/monte-carlo-risk-assessment-mcra)
