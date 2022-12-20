using System.Globalization;
using System.IO.Compression;
using System.Xml.Serialization;
using MCRA.Data.Raw;
using MCRA.General;
using MCRA.General.Action.Serialization;
using MCRA.General.Action.Settings;
using MCRA.General.Action.Settings.Dto;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Xml;

namespace MCRA.Data.Management {
    public abstract class ImportManagerBase : IDisposable {

        private bool _disposed;
        protected IRawDataManager _rawDataManager;

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
        /// <param name="dataManager"></param>
        public ImportManagerBase(IRawDataManager dataManager) {
            _rawDataManager = dataManager;
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ImportManagerBase()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Imports a project zip file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public (ProjectDto settings, DataSourceConfiguration dsConfig) ImportZipFile(
            string fileName,
            CompositeProgressState progressState) {
            // Get the XML content for settings and data source configuration
            var settingsXml = "";
            var dsConfigXml = "";

            // Boolean to determine whether to use 'old style' zip file format
            // containing full data source files
            var oldStyle = false;
            var workingFolder = "";

            // Boolean to determine if the zip file has a data folder with files
            // otherwise it's considered an uploadable data source version itself, containing
            // data as CSV files
            var hasDataFolder = false;
            var hasDataConfig = false;

            var zipFileInfo = new FileInfo(fileName);

            // Open the zip archive, determine type of file
            using (var zip = ZipFile.Open(fileName, ZipArchiveMode.Read)) {
                ZipArchiveEntry settingsEntry = null;
                ZipArchiveEntry dsConfigEntry = null;

                foreach (var e in zip.Entries) {
                    if (e.Name.Equals("ProjectSettings.xml", StringComparison.OrdinalIgnoreCase)) {
                        settingsEntry = e;
                        oldStyle = true;
                    } else if (e.Name.Equals("ProjectDataSources.xml", StringComparison.OrdinalIgnoreCase)) {
                        dsConfigEntry = e;
                        oldStyle = true;
                        hasDataConfig = true;
                    } else if (e.Name.Equals("_ActionSettings.xml", StringComparison.OrdinalIgnoreCase)) {
                        settingsEntry = e;
                    } else if (e.Name.Equals("_ActionData.xml", StringComparison.OrdinalIgnoreCase)) {
                        dsConfigEntry = e;
                        hasDataConfig = true;
                    } else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(e.FullName, @"/Data/") >= 0) {
                        hasDataFolder = true;
                    } else if (e.FullName.StartsWith(@"Data/", StringComparison.InvariantCultureIgnoreCase)) {
                        hasDataFolder = true;
                    } else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(e.FullName, @"\Data\") >= 0) {
                        hasDataFolder = true;
                    } else if (e.FullName.StartsWith(@"Data\", StringComparison.InvariantCultureIgnoreCase)) {
                        hasDataFolder = true;
                    }
                }

                // If mandatory config files don't exist, this is not a valid zip file
                if (settingsEntry == null || ((oldStyle || hasDataConfig) && dsConfigEntry == null)) {
                    throw new Exception("Not a validly recognized zip file");
                }

                // Read action/project settings XML
                using (var xmlStream = settingsEntry.Open())
                using (var reader = new StreamReader(xmlStream)) {
                    settingsXml = reader.ReadToEnd();
                }

                if (hasDataConfig) {
                    // Read data source configuration XML
                    using (var xmlStream = dsConfigEntry.Open())
                    using (var reader = new StreamReader(xmlStream)) {
                        dsConfigXml = reader.ReadToEnd();
                    }
                }
                var fullName = settingsEntry.FullName;
                workingFolder = fullName.Length > settingsEntry.Name.Length
                                ? fullName.Substring(0, fullName.Length - settingsEntry.Name.Length - 1)
                                : "";
            }

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

                if (!hasDataFolder && hasDataConfig) {
                    //Zip file itself is the data file, set the correct settings and return
                    foreach (var mapping in dsMappings) {
                        mapping.Name = zipFileInfo.Name;
                        mapping.RawDataSourcePath = zipFileInfo.Name;
                        mapping.IdRawDataSourceVersion = 1;
                    }
                }
            }

            //save the uploaded file in a temporary upload directory, use a guid for uniqueness
            var token = $"Unzip{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            var unpackDirInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), token));

            try {
                //unpack the contents of the zip file to the temporary folder
                using (var zip = ZipFile.Open(fileName, ZipArchiveMode.Read)) {
                    //unpack to unpack dir
                    zip.ExtractToDirectory(unpackDirInfo.FullName);
                }

                var projDir = Path.Combine(unpackDirInfo.FullName, workingFolder);
                //check whether zip file contains one folder with the project name itself
                //then use that, otherwise the extraction folder itself
                var projectFolder = Directory.Exists(projDir) ? projDir : unpackDirInfo.FullName;

                var diData = new DirectoryInfo(Path.Combine(projectFolder, "Data"));

                //add all files in Data subfolder
                var rawFiles = hasDataFolder ? diData.GetFiles() : new[] { new FileInfo(fileName) };
                var dsIdLookup = rawFiles.ToDictionary(fi => fi.Name, fi => (IRawDataSourceVersion)null, StringComparer.OrdinalIgnoreCase);
                var sourceTableGroups = new Dictionary<SourceTableGroup, HashSet<IRawDataSourceVersion>>();

                //if there are any entries in the mapping records for which there is no data file
                //throw an exception
                var pathFromName = true;
                var dsFileNames = hasDataConfig
                                ? dsMappings.Select(ds => ds.Name).ToHashSet(StringComparer.OrdinalIgnoreCase)
                                : new() { zipFileInfo.Name };

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
                        // Calculate file size in kilobytes
                        version.FileSizeKb = Convert.ToSingle(sourceFileInfo.Length) / 1024F;

                        // Copy the data to the database
                        version.TableGroups = _rawDataManager
                            .LoadDataSourceFileIntoDb(version, progressState ?? new CompositeProgressState())
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
                return (projectSettings, dsConfigNew);

            } catch (Exception) {
                throw;
            } finally {
                try { unpackDirInfo.Delete(true); } catch { /* no action */ }
            }
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                }
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
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
