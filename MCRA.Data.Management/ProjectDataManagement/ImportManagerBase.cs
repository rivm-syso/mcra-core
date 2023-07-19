using System.IO.Compression;
using System.Xml.Serialization;
using MCRA.Data.Raw;
using MCRA.General;
using MCRA.General.Action.Serialization;
using MCRA.General.Action.Settings;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Xml;

namespace MCRA.Data.Management {
    public abstract class ImportManagerBase : IDisposable {

        private bool _disposed;
        protected readonly IRawDataManager _rawDataManager;

        #region old style project+data zip files

        //Private classes for loading custom datasource XML
        public class ProjectDataSource {
            [XmlElement("FileName")]
            public string FileName { get; set; } = string.Empty;
            [XmlElement("TableGroup")]
            public SourceTableGroup TableGroup { get; set; }
            [XmlElement("Checksum")]
            public string Checksum { get; set; } = string.Empty;
        }

        [XmlRoot("ProjectDataSources")]
        public class ProjectDataSources {
            [XmlElement("ProjectDataSource")]
            public ProjectDataSource[] DataSources { get; set; }
        }
        #endregion

        /// <summary>
        /// Creates a new <see cref="ImportManagerBase"/> instance.
        /// </summary>
        /// <param name="rawDataManager"></param>
        public ImportManagerBase(IRawDataManager rawDataManager) {
            _rawDataManager = rawDataManager;
        }

        /// <summary>
        /// Imports a project from folder.
        /// </summary>
        /// <param name="actionFolder"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public (ProjectDto settings, DataSourceConfiguration dsConfig) ImportAction(
            string actionFolder,
            CompositeProgressState progress
        ) {
            var localProgress = progress?.NewProgressState(1) ?? new ProgressState();

            // Get the XML content for settings and data source configuration
            var settingsXml = string.Empty;
            var dsConfigXml = string.Empty;

            // Boolean to determine whether to use 'old style' zip file format
            // containing full data source files
            var oldStyle = false;
            var workingFolder = string.Empty;

            // Boolean to determine if the zip file has a data folder with files
            // otherwise it's considered an uploadable data source version itself, containing
            // data as CSV files
            var hasDataFolder = false;
            var hasDataConfig = false;

            string actionSettingsFile = string.Empty;
            string actionDataFile = string.Empty;

            string tempZippedCsvFilePath = string.Empty;

            var actionFolderName = actionFolder.Substring(Directory.GetParent(actionFolder).ToString().Length).Trim('\\');
            var actionFiles = Directory.GetFileSystemEntries(actionFolder, "*", SearchOption.AllDirectories);
            foreach (var file in actionFiles) {
                if (Path.GetFileName(file).Equals("ProjectSettings.xml", StringComparison.OrdinalIgnoreCase)) {
                    actionSettingsFile = file;
                    oldStyle = true;
                } else if (Path.GetFileName(file).Equals("ProjectDataSources.xml", StringComparison.OrdinalIgnoreCase)) {
                    actionDataFile = file;
                    oldStyle = true;
                    hasDataConfig = true;
                } else if (Path.GetFileName(file).Equals("_ActionSettings.xml", StringComparison.OrdinalIgnoreCase)) {
                    actionSettingsFile = file;
                } else if (Path.GetFileName(file).Equals("_ActionData.xml", StringComparison.OrdinalIgnoreCase)) {
                    actionDataFile = file;
                    hasDataConfig = true;
                } else if (file.EndsWith($"{actionFolderName}\\Data", StringComparison.InvariantCultureIgnoreCase)) {
                    hasDataFolder = true;
                }
            }

            // If mandatory config files don't exist, this is not a valid action
            if (string.IsNullOrEmpty(actionSettingsFile) || ((oldStyle || hasDataConfig) && string.IsNullOrEmpty(actionDataFile))) {
                throw new Exception("Not a validly recognized zip file");
            }

            // Read action settings XML
            using (var reader = new StreamReader(actionSettingsFile)) {
                settingsXml = reader.ReadToEnd();
            }

            // Read action data XML (data source configuration), if any
            if (hasDataConfig) {
                using (var reader = new StreamReader(actionDataFile)) {
                    dsConfigXml = reader.ReadToEnd();
                }
            }
            var fullName = actionSettingsFile;
            //workingFolder = fullName.Length > settingsEntry.Name.Length
            //                ? fullName.Substring(0, fullName.Length - settingsEntry.Name.Length - 1)
            //                : string.Empty;
            workingFolder = string.Empty;
            //}

            DataSourceConfiguration dsConf;
            List<DataSourceMappingRecord> dsMappings;

            if (oldStyle) {
                //read old style data source config xml into private types
                //and convert to DataSourceConfiguration
                //only import table groups that have a valid (non-empty) filename
                var projectFiles = XmlSerialization.FromXml<ProjectDataSources>(dsConfigXml);
                dsMappings = projectFiles.DataSources
                    .Where(ds => !string.IsNullOrWhiteSpace(ds.FileName))
                    .Select(ds =>
                        new DataSourceMappingRecord {
                            Name = ds.FileName,
                            RawDataSourcePath = ds.FileName,
                            Checksum = ds.Checksum,
                            SourceTableGroup = ds.TableGroup
                        }
                    ).ToList();

                dsConf = new DataSourceConfiguration();

                foreach (var mapping in dsMappings) {
                    //also set input mapping record's datasource id to the new raw DS id
                    dsConf.SetTableGroupDataSource(mapping.SourceTableGroup, mapping.IdRawDataSourceVersion);
                }

            } else {
                if (hasDataConfig) {
                    dsConf = XmlSerialization.FromXml<DataSourceConfiguration>(dsConfigXml);
                } else {
                    dsConf = new DataSourceConfiguration();
                }
                dsMappings = dsConf.DataSourceMappingRecords;

                if (!hasDataFolder) {
                    //Folder with csv files itself is considered as a data container file
                    tempZippedCsvFilePath = CreateTempCsvZipFile(actionFolder, actionFolderName);
                    if (hasDataConfig) { 
                        foreach (var mapping in dsMappings) {
                             mapping.Name = Path.GetFileName(tempZippedCsvFilePath);
                             mapping.RawDataSourcePath = Path.GetFileName(tempZippedCsvFilePath);
                             mapping.IdRawDataSourceVersion = 1;
                        }
                    }
                }
            }

            //save the uploaded file in a temporary upload directory, use a guid for uniqueness
            var token = $"Unzip{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            var unpackDirInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), token));

            try {
                var projDir = Path.Combine(actionFolder, workingFolder);
                //check whether zip file contains one folder with the project name itself
                //then use that, otherwise the extraction folder itself
                var projectFolder = Directory.Exists(projDir) ? projDir : unpackDirInfo.FullName;

                var diData = new DirectoryInfo(Path.Combine(projectFolder, "Data"));

                //add all data container files (.zip, .xlsx, .mdb)
                var rawFiles = hasDataFolder ? diData.GetFiles() : new[] { new FileInfo(tempZippedCsvFilePath) };
                var dsIdLookup = rawFiles.ToDictionary(fi => fi.Name, fi => (IRawDataSourceVersion)null, StringComparer.OrdinalIgnoreCase);
                var sourceTableGroups = new Dictionary<SourceTableGroup, HashSet<IRawDataSourceVersion>>();

                //if there are any entries in the mapping records for which there is no data file
                //throw an exception
                var pathFromName = true;
                var dsFileNames = hasDataConfig
                                ? dsMappings.Select(ds => ds.Name).ToHashSet(StringComparer.OrdinalIgnoreCase)
                                : new() { Path.GetFileName(tempZippedCsvFilePath) };

                if (hasDataConfig && dsFileNames.Any(fn => !dsIdLookup.ContainsKey(fn))) {
                    // for older files, the RawDataSourcePath is used
                    dsFileNames = dsMappings.Select(ds => ds.RawDataSourcePath).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    pathFromName = false;
                    if (dsFileNames.Any(fn => !dsIdLookup.ContainsKey(fn))) {
                        throw new Exception("The data source configuration contains reference(s) to a data file " +
                                        "that is not available in the Data folder of the zip file.");
                    }
                }

                var rawDsVersionId = 1;
                var fileCopyProgressStepSize = 99 / rawFiles.Length;
                foreach (var file in rawFiles) {
                    if (dsFileNames.Contains(file.Name)) {

                        var sourceFileInfo = new FileInfo(file.FullName);
                        var version = new RawDataSourceVersionDto() {
                            id = rawDsVersionId++,
                            UploadTimestamp = DateTime.Now,
                            Name = sourceFileInfo.Name,
                            FullPath = file.FullName,
                            DataSourcePath = file.DirectoryName,
                            VersionNumber = 1
                        };

                        // Calculate checksum
                        version.Checksum = DataSourceReaderBase.CalculateFileHashBase64(file.FullName);

                        // Copy the data to the database
                        var fileCopyProgress = progress?.NewCompositeState(fileCopyProgressStepSize) ?? new CompositeProgressState();
                        version.TableGroups = _rawDataManager
                            .LoadDataSourceFileIntoDb(version, fileCopyProgress)
                            .ToHashSet();

                        if (!hasDataConfig) {
                            //add data source configuration mapping for every tablegroup
                            dsMappings.AddRange(version.TableGroups.Select(
                                g => new DataSourceMappingRecord {
                                    Name = file.Name,
                                    RawDataSourcePath = file.Name,
                                    IdRawDataSourceVersion = 1,
                                    SourceTableGroup = g
                                }
                            ));
                        }
                        version.DataIsInDatabase = true;
                        dsIdLookup[file.Name] = version;

                        if (oldStyle) {
                            //source table groups backward compatibility
                            //in old datasource configs, certain tables didn't have their own table group
                            //if for any of these table groups data is available,
                            //store the table id for this group in a dictionary
                            //if multiple files contain same group, register this
                            //exclude certain table groups for which
                            //explicit configuration should be set
                            var tableGroupExceptions = new HashSet<SourceTableGroup> {
                                SourceTableGroup.Unknown,
                                SourceTableGroup.NonDietary,
                                SourceTableGroup.FocalFoods,
                                SourceTableGroup.HumanMonitoringData,
                                SourceTableGroup.Populations,
                                SourceTableGroup.TotalDietStudy
                            };
                            foreach (var stg in version.TableGroups.Where(g => !tableGroupExceptions.Contains(g))) {
                                if (!sourceTableGroups.TryGetValue(stg, out var rdvList)) {
                                    rdvList = new HashSet<IRawDataSourceVersion>();
                                    sourceTableGroups.Add(stg, rdvList);
                                }
                                sourceTableGroups[stg].Add(version);
                            }
                        }
                        fileCopyProgress.MarkCompleted();
                    }
                }

                var dsConfigNew = new DataSourceConfiguration();

                foreach (var mapping in dsMappings) {
                    //also set input mapping record's datasource id to the new raw DS id
                    mapping.IdRawDataSourceVersion = dsIdLookup[pathFromName ? mapping.Name : mapping.RawDataSourcePath].id;
                    dsConfigNew.AppendTableGroupDataSource(mapping.SourceTableGroup, mapping.IdRawDataSourceVersion);
                }

                //backward compatibility: also hook up table groups for which data is in the files
                //but was not read in yet.
                if (oldStyle) {
                    //dictionary of mappings between new data sources to the data sources in which
                    //the tables are (usually, or preferrably) found in older projects, only used when there are
                    //multiple candidate data sources available for the table group to be mapped
                    var preferredMappings = new Dictionary<SourceTableGroup, SourceTableGroup> {
                        { SourceTableGroup.FoodTranslations, SourceTableGroup.Foods },
                        { SourceTableGroup.MarketShares, SourceTableGroup.Foods },
                        { SourceTableGroup.MaximumResidueLimits, SourceTableGroup.Concentrations },
                        { SourceTableGroup.RelativePotencyFactors, SourceTableGroup.Concentrations },
                        { SourceTableGroup.HazardDoses, SourceTableGroup.Concentrations },
                    };

                    foreach (var kvp in sourceTableGroups.Where(k => !dsConfigNew.HasDataGroup(k.Key))) {
                        //When multiple data sources are found, use the preferred mappings to find
                        //the data source to use, otherwise use the first one found when there is no
                        //preference mapping
                        //default: use first value in the list
                        var useDs = kvp.Value.First();
                        if (kvp.Value.Count > 1 && preferredMappings.TryGetValue(kvp.Key, out var preferredGroup)) {
                            var preferredDs = kvp.Value.FirstOrDefault(d => d.ContainsSourceTableGroup(preferredGroup));
                            if (preferredDs != null) {
                                useDs = preferredDs;
                            }
                        }
                        dsConfigNew.SetTableGroupDataSource(kvp.Key, useDs.id);
                    }
                }
                var projectSettings = ProjectSettingsSerializer.ImportFromXmlString(settingsXml, dsConfigNew, oldStyle, out _);

                localProgress.Update("Done!", 100);

                return (projectSettings, dsConfigNew);
            } catch (Exception) {
                throw;
            } finally {
                try {
                    if (File.Exists(tempZippedCsvFilePath)) {
                        File.Delete(tempZippedCsvFilePath);
                    }
                } catch { /* no action */ }
            }
        }

        /// <summary>
        /// Creates a temporary zip file from a folder that contains CSV files. 
        /// </summary>
        /// <returns>Full path to temporary zip file.</returns>
        private static string CreateTempCsvZipFile(string actionFolder, string actionFolderName) {
            var zippedCsvFileName = $"{actionFolderName}-{Guid.NewGuid().ToString("N").Substring(0, 8)}.zip";
            var path = Path.GetTempPath();
            var zippedCsvFilePath = Path.Combine(path, zippedCsvFileName);

            var csvFiles = Directory.GetFiles(actionFolder, "*.csv", SearchOption.TopDirectoryOnly);
            using (var zipArchive = ZipFile.Open(zippedCsvFilePath, ZipArchiveMode.Create)) {
                foreach (var file in csvFiles) {
                    var fileInfo = new FileInfo(file);
                    zipArchive.CreateEntryFromFile(fileInfo.FullName, fileInfo.Name);
                }
            }
            return zippedCsvFilePath;
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    _rawDataManager.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
