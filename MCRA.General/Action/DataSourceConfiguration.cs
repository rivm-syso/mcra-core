using MCRA.General.Action.Settings;

namespace MCRA.General {

    /// <summary>
    /// Defines a selection of raw data sources.
    /// </summary>
    public sealed class DataSourceConfiguration {

        public List<DataSourceMappingRecord> DataSourceMappingRecords { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DataSourceConfiguration() {
            DataSourceMappingRecords = [];
        }

        /// <summary>
        /// Create new instance from a dictionary, <see cref="IDictionary<SourceTableGroup, IList<IRawDataSourceVersion>>"/>
        /// </summary>
        /// <param name="versions">dictionary to create the instance from</param>
        public DataSourceConfiguration (IDictionary<SourceTableGroup, List<IRawDataSourceVersion>> versions) {
            DataSourceMappingRecords = versions
            .SelectMany(
                d => d.Value,
                (kvp, dv) => new DataSourceMappingRecord {
                    IdRawDataSourceVersion = dv.id,
                    RawDataSourcePath = dv.DataSourcePath,
                    Name = dv.DataSourceName,
                    SourceTableGroup = kvp.Key,
                    Checksum = dv.Checksum,
                    RepositoryPath = dv.DataSourcePath
                }
            ).ToList();
        }

        /// <summary>
        /// Returns the data source mapping records as a dictionary of data source versions per source table group
        /// </summary>
        /// <returns></returns>
        public IDictionary<SourceTableGroup, List<IRawDataSourceVersion>> ToVersionsDictionary() {
            return DataSourceMappingRecords
                .GroupBy(m => m.SourceTableGroup)
                    .ToDictionary(g => g.Key, g => g.Select(m => new RawDataSourceVersion {
                        id = m.IdRawDataSourceVersion,
                        FullPath = string.IsNullOrEmpty(m.RepositoryPath)
                                 ? m.Name
                                 : Path.Combine(m.RepositoryPath, m.Name),
                        DataIsInDatabase = false,
                        Checksum = m.Checksum,
                        Name = m.Name,
                        DataSourceName = m.Name,
                        UploadTimestamp = null,
                        VersionNumber = 1,
                        DataSourcePath = m.RawDataSourcePath
                    })
                    .Cast<IRawDataSourceVersion>()
                    .ToList()
                );
        }

        /// <summary>
        /// Returns whether the data source configuration contains a mapping for the
        /// specified source table group.
        /// </summary>
        /// <param name="sourceTableGroup"></param>
        /// <returns></returns>
        public bool HasDataGroup(SourceTableGroup sourceTableGroup) {
            return DataSourceMappingRecords.Any(r => r.SourceTableGroup == sourceTableGroup);
        }

        /// <summary>
        /// Returns the id of the raw data source for the specified table group, or null if no mapping exists
        /// for this source table group.
        /// </summary>
        /// <param name="sourceTableGroup"></param>
        /// <returns></returns>
        public int[] GetRawDataSourceIds(SourceTableGroup sourceTableGroup) {
            return DataSourceMappingRecords
                .Where(r => r.SourceTableGroup == sourceTableGroup)
                .Select(r => r.IdRawDataSourceVersion)
                .ToArray();
        }

        /// <summary>
        /// Sets the specified data source for the specified source table group.
        /// </summary>
        /// <param name="sourceTableGroup"></param>
        /// <param name="idRawDataSourceVersion"></param>
        public void SetTableGroupDataSource(SourceTableGroup sourceTableGroup, int? idRawDataSourceVersion) {
            var mapping = DataSourceMappingRecords.FirstOrDefault(r => r.SourceTableGroup == sourceTableGroup);
            if (idRawDataSourceVersion == null) {
                DataSourceMappingRecords.RemoveAll(r => r.SourceTableGroup == sourceTableGroup);
            } else if (mapping != null) {
                mapping.IdRawDataSourceVersion = (int)idRawDataSourceVersion;
            } else {
                DataSourceMappingRecords.Add(new DataSourceMappingRecord() {
                    SourceTableGroup = sourceTableGroup,
                    IdRawDataSourceVersion = (int)idRawDataSourceVersion
                });
            }
        }

        /// <summary>
        /// Sets the specified data source for the specified source table group.
        /// </summary>
        /// <param name="sourceTableGroup"></param>
        /// <param name="idRawDataSourceVersion"></param>
        public void AppendTableGroupDataSource(SourceTableGroup sourceTableGroup, int idRawDataSourceVersion) {
            if (!DataSourceMappingRecords.Any(r => r.SourceTableGroup == sourceTableGroup && r.IdRawDataSourceVersion == idRawDataSourceVersion)) {
                DataSourceMappingRecords.Add(new DataSourceMappingRecord() {
                    SourceTableGroup = sourceTableGroup,
                    IdRawDataSourceVersion = idRawDataSourceVersion
                });
            }
        }

        /// <summary>
        /// Sets the specified data source for all specified data groups. When no data groups
        /// are specified, it is assumed that the data source should be set for all data groups
        /// except the focal foods data group and the total diet study data group.
        /// </summary>
        /// <param name="rawDataSourceVersion"></param>
        /// <param name="dataGroups"></param>
        public void Set(IRawDataSourceVersion rawDataSourceVersion, params SourceTableGroup[] dataGroups) {
            if (dataGroups.Length == 0) {
                dataGroups = SelectAllTableGroups;
            }
            foreach (var tableGroup in dataGroups) {
                if (rawDataSourceVersion.TableGroups.Any(r => r == tableGroup)) {
                    SetTableGroupDataSource(tableGroup, rawDataSourceVersion.id);
                }
            }
        }

        public static SourceTableGroup[] SelectAllTableGroups {
            get {
                var tableGroups = Enum.GetValues(typeof(SourceTableGroup))
                    .Cast<SourceTableGroup>()
                    .Where(r => r != SourceTableGroup.FocalFoods)
                    .ToArray();
                return tableGroups;
            }
        }
    }
}
