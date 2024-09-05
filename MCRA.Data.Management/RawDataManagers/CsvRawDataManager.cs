using MCRA.Data.Raw;
using MCRA.Data.Raw.Constants;
using MCRA.Data.Raw.Copying;
using MCRA.General;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.DataSourceReading.DataSourceReaders;
using MCRA.Utils.ProgressReporting;
using System.Data;
using System.Diagnostics;

namespace MCRA.Data.Management.RawDataManagers {
    public class CsvRawDataManager : IRawDataManager {

        private readonly DirectoryInfo _baseDataFolder;
        private readonly Dictionary<int, DirectoryInfo> _dataSourceFolders = new();

        public CsvRawDataManager(string dataFolderName) {
            _baseDataFolder = new DirectoryInfo(dataFolderName);
            if (!_baseDataFolder.Exists) {
                _baseDataFolder.Create();
                _baseDataFolder.Refresh();
            }
        }

        /// <summary>
        /// Returns a data reader for the raw data source with the specified id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idRawDataSource"></param>
        /// <param name="fieldMap"></param>
        /// <returns></returns>
        public IDataReader OpenDataReader<T>(int idRawDataSource, out int[] fieldMap) where T : IConvertible {
            // For the following raw data objects no CSV mapping exists
            // TODO: this tabu list should not exist, this check should not be done here!
            var tabuTypes = new List<Type>() {
                typeof(RawSampleYears),
                typeof(RawSampleLocations),
                typeof(RawSampleRegions),
                typeof(RawSampleProductionMethods),
                typeof(RawTwoWayTableData)
            };
            var tableId = RawTableIdToFieldEnums.EnumToIdMap[typeof(T)];
            if (tabuTypes.Any(r => typeof(T) == r)) {
                // Return null if the generic type is a tabu type
                fieldMap = null;
                return null;
            } else {
                var reader = OpenDataReader(idRawDataSource, tableId, out fieldMap);
                return reader;
            }
        }

        /// <summary>
        /// Opens a data reader for the specified raw data source and table.
        /// </summary>
        /// <param name="idRawDataSource"></param>
        /// <param name="tableId"></param>
        /// <param name="fieldMap"></param>
        /// <returns></returns>
        public IDataReader OpenDataReader(int idRawDataSource, RawDataSourceTableID tableId, out int[] fieldMap) {
            fieldMap = null;
            if (McraTableDefinitions.Instance.TableFieldsMap.TryGetValue(tableId, out TableFieldsMap dto)) {
                var reader = getOpenDataReader(idRawDataSource, dto.EnumType.Name, tableId);
                if (reader != null) {
                    var tableDef = McraTableDefinitions.Instance.GetTableDefinition(tableId);
                    fieldMap = tableDef.ColumnDefinitions.GetColumnMappings(reader.GetColumnNames());
                } else {
                    fieldMap = null;
                }
                return reader;
            }
            throw new Exception($"No reader specified for table {tableId}.");
        }

        /// <summary>
        /// Opens a keys reader for the specified data source and table type 
        /// containing the primary key, name, and the foreign key references.
        /// </summary>
        /// <param name="idRawDataSource"></param>
        /// <param name="idRawTable"></param>
        /// <param name="linkedTables"></param>
        /// <returns></returns>
        public IDataReader OpenKeysReader(
            int idRawDataSource,
            RawDataSourceTableID idRawTable,
            params (RawDataSourceTableID TableId, string KeyField)[] linkedTables
        ) {
            if (!CheckRawDataSourceAvailable(idRawDataSource)) {
                return null;
            }
            if (McraTableDefinitions.Instance.TableFieldsMap.TryGetValue(idRawTable, out TableFieldsMap tableFieldsMap)
                && _dataSourceFolders.TryGetValue(idRawDataSource, out var dataFolder)
            ) {
                var tableName = tableFieldsMap.EnumType.Name;
                if (!File.Exists(Path.Combine(dataFolder.FullName, $"{tableName}.csv"))) {
                    return null;
                }
                var fieldNames = new List<string> {
                     tableFieldsMap.PrimaryKeyField?.ToString(),
                     tableFieldsMap.NameField?.ToString()
                };
                if (linkedTables?.Any() ?? false) {
                    fieldNames.AddRange(linkedTables.Select(l => l.KeyField));
                }

                try {
                    var dataReader = getOpenDataReader(idRawDataSource, tableName, idRawTable);
                    var columnMappings = fieldNames
                        .Select(n => n == null ? 0 : dataReader.GetOrdinal(n))
                        .ToArray();
                    var mappedReader = new MappedColumnDataReader(dataReader, columnMappings);
                    return mappedReader;
                } catch (Exception ex) {
                    Debug.WriteLine(ex.ToString());
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Copies the data in the datasource file to the 'Raw' tables in the backend database.
        /// Executes only if the data has not already been loaded.
        /// </summary>
        /// <param name="rds"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public HashSet<SourceTableGroup> LoadDataSourceFileIntoDb(
            IRawDataSourceVersion rds,
            CompositeProgressState progressState
        ) {
            if (!rds.DataIsInDatabase) {
                var dataSourceFolder = new DirectoryInfo(Path.Combine(_baseDataFolder.FullName, rds.Name));
                _dataSourceFolders[rds.id] = dataSourceFolder;
                using var dataSourceWriter = new CsvDataSourceWriter(dataSourceFolder);
                var bulkCopier = new RawDataSourceBulkCopier(dataSourceWriter);
                bulkCopier.CopyFromDataFile(rds.FullPath, rds: rds, progressState: progressState);
                if (!rds.ContainsSourceTableGroup()) {
                    throw new RawDataSourceBulkCopyException("The uploaded database does not contain any recognized source tables.");
                }
            }
            return rds.TableGroups.ToHashSet();
        }

        /// <summary>
        /// Copies the data in the dataTables to the 'Raw' tables in the backend database.
        /// </summary>
        /// <param name="dataTables"></param>
        /// <param name="rds"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        /// <exception cref="RawDataSourceBulkCopyException"></exception>
        public HashSet<SourceTableGroup> LoadDataTablesIntoDb(
            DataTable[] dataTables,
            IRawDataSourceVersion rds,
            CompositeProgressState progressState
        ) {
            if (!rds.DataIsInDatabase) {
                var dataSourceFolder = new DirectoryInfo(Path.Combine(_baseDataFolder.FullName, rds.Name));
                _dataSourceFolders[rds.id] = dataSourceFolder;
                using var dataSourceWriter = new CsvDataSourceWriter(dataSourceFolder);
                var bulkCopier = new RawDataSourceBulkCopier(dataSourceWriter);
                bulkCopier.CopyFromDataTables(dataTables, rds: rds, progressState: progressState);
                if (!rds.ContainsSourceTableGroup()) {
                    throw new RawDataSourceBulkCopyException("The uploaded database does not contain any recognized source tables.");
                }
            }
            return rds.TableGroups.ToHashSet();
        }

        /// <summary>
        /// Implements <see cref="IRawDataManager.CheckRawDataSourceAvailable(int)"/>.
        /// </summary>
        /// <param name="idRawDataSource"></param>
        /// <returns></returns>
        public bool CheckRawDataSourceAvailable(int idRawDataSource) {
            return _dataSourceFolders.TryGetValue(idRawDataSource, out var fi) && fi.GetFiles().Any();
        }

        private IDataReader getOpenDataReader(
            int idRawDataSource,
            string tableName,
            RawDataSourceTableID tableId
        ) {
            if (!CheckRawDataSourceAvailable(idRawDataSource)) {
                return null;
            }
            try {
                if (_dataSourceFolders.TryGetValue(idRawDataSource, out var dataFolder)) {
                    var reader = new CsvFolderReader(dataFolder.FullName);
                    reader.Open();
                    var tableDef = McraTableDefinitions.Instance.GetTableDefinition(tableId);
                    var dataReader = reader.GetDataReaderByName(tableName, tableDef);
                    return dataReader;
                }
                return null;
            } catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing) {
            // Dispose pattern is not implemented here
            // TODO: refactor: CompiledDataManager uses the datamanager in a using statement
            // this is not necessary for this class
        }

        public void Dispose() {
            // Dispose pattern is not implemented here
            // TODO: refactor: CompiledDataManager uses the datamanager in a using statement
            // this is not necessary for this class
        }

        #endregion

        public (string Name, string Size) GetDatabaseInfo() {
            throw new NotImplementedException();
        }

        public IDictionary<string, string> GetTableInfo() {
            throw new NotImplementedException();
        }
    }
}
