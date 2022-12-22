# MCRA zipped CSV dataset template [TableGroup]

This ZIP file is an empty template of an MCRA compliant dataset for [TableGroup]. MCRA datasets are composed of one or more relational tables describing the data. Depending on the type of data, some tables may be required and others may be optional. For some types of data, multiple ways of formatting are available, each with way with its own particular requirements. For a detailed description of the data format for [TableGroup], go to:

https://mcra.rivm.nl/Mcra91/WebApp/manual/modules/[ModuleClassId]-modules/[ModuleId]/[ModuleId]-data-formats.html#consumptions-data-formats

In the zipped CSV format, each data table is specified in a comma-separated text file (CSV). The names of the CSV files in the zip archive must follow the accepted table names of the module(s) for which the data is intended. The table fields in the CSV files must follow the data table formats of the intended modules.

You can use this empty template to format own data to upload and use it in MCRA. This can be done by unzipping it in some folder on your own machine, adding the data in the CSV files (e.g., by editing it with Microsoft Excel), and, when ready, creating a ZIP file from the folder again. You can delete the CSV files of the tables that are not needed/required by deleting respective the zip files. Also, columns that are not required may be removed from the CSV files. The resulting ZIP file can be uploaded to MCRA, which should recognize it as a valid data file containing [TableGroup].

For more details on creating and uploading MCRA datasets, and information on other accepted file formats, go to:

https://mcra.rivm.nl/Mcra91/WebApp/manual/introduction/data-repository/index.html#creating-and-uploading-data-files
