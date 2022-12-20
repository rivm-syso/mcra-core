using MCRA.Data.Raw;
using MCRA.Data.Raw.Constants;
using MCRA.Data.Raw.Copying;
using MCRA.General;
using MCRA.General.TableDefinitions;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.DataSourceReading.DataSourceReaders;
using MCRA.Utils.ProgressReporting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace MCRA.Data.Management.RawDataManagers {
    public class BinaryRawDataManager : IRawDataManager {

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

        private readonly DirectoryInfo _baseDataFolder;
        private readonly Dictionary<int, DirectoryInfo> _dataSourceFolders = new Dictionary<int, DirectoryInfo>();

        public BinaryRawDataManager(string dataFolderName) {
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
            var tableId = RawTableIdToFieldEnums.EnumToIdMap[typeof(T)];
            var reader = OpenDataReader(idRawDataSource, tableId, out fieldMap);
            return reader;
        }

        /// <summary>
        /// Opens a data reader for the specified raw data source and table.
        /// </summary>
        /// <param name="idRawDataSource"></param>
        /// <param name="idRawTable"></param>
        /// <param name="fieldMap"></param>
        /// <returns></returns>
        public IDataReader OpenDataReader(int idRawDataSource, RawDataSourceTableID idRawTable, out int[] fieldMap) {
            fieldMap = null;
            if (McraTableDefinitions.Instance.TableFieldsMap.TryGetValue(idRawTable, out TableFieldsMap dto)) {
                return getOpenDataReader(idRawDataSource, dto.EnumType.Name, idRawTable);
            }
            throw new Exception($"No reader specified for table {idRawTable}.");
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
            params (RawDataSourceTableID, string)[] linkedTables
        ) {
            if (!CheckRawDataSourceAvailable(idRawDataSource)) {
                return null;
            }
            if (McraTableDefinitions.Instance.TableFieldsMap.TryGetValue(idRawTable, out TableFieldsMap tableFieldsMap)
                && _dataSourceFolders.TryGetValue(idRawDataSource, out var dataFolder)
            ) {
                var tableName = tableFieldsMap.EnumType.Name;
                if(!File.Exists(Path.Combine(dataFolder.FullName, $"{tableName}.bin"))) {
                    return null;
                }
                var fieldNames = new List<string> {
                     tableFieldsMap.PrimaryKeyField?.ToString(),
                     tableFieldsMap.NameField?.ToString()
                };
                if (linkedTables?.Any() ?? false) {
                    fieldNames.AddRange(linkedTables.Select(l => l.Item2));
                }

                try {
                    var dataReader = getOpenDataReader(idRawDataSource, tableName, idRawTable);
                    var columnMappings = fieldNames
                        .Select(n => n == null ? 0 : dataReader.GetOrdinal(n))
                        .ToArray();
                    var mappedReader = new MappedColumnDataReader(dataReader, columnMappings);
                    return mappedReader;
                } catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
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
        public ICollection<SourceTableGroup> LoadDataSourceFileIntoDb(
            IRawDataSourceVersion rds,
            CompositeProgressState progressState
        ) {
            if (!rds.DataIsInDatabase) {
                var dataSourceFolder = new DirectoryInfo(Path.Combine(_baseDataFolder.FullName, rds.Name));
                _dataSourceFolders[rds.id] = dataSourceFolder;
                var dataSourceWriter = new BinaryDataSourceWriter(dataSourceFolder);
                var bulkCopier = new RawDataSourceBulkCopier(dataSourceWriter);
                bulkCopier.CopyFromDataFile(rds.FullPath, rds: rds, progressState: progressState);
                if (!rds.ContainsSourceTableGroup()) {
                    throw new RawDataSourceBulkCopyException("The uploaded database does not contain any recognized source tables.");
                }
            }
            return rds.TableGroups.ToList();
        }

        /// <summary>
        /// Copies the data in the dataTables to the 'Raw' tables in the backend database.
        /// </summary>
        /// <param name="dataTables"></param>
        /// <param name="rds"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        /// <exception cref="RawDataSourceBulkCopyException"></exception>
        public ICollection<SourceTableGroup> LoadDataTablesIntoDb(
            DataTable[] dataTables,
            IRawDataSourceVersion rds,
            CompositeProgressState progressState
        ) {
            if (!rds.DataIsInDatabase) {
                var dataSourceFolder = new DirectoryInfo(Path.Combine(_baseDataFolder.FullName, rds.Name));
                _dataSourceFolders[rds.id] = dataSourceFolder;
                var dataSourceWriter = new BinaryDataSourceWriter(dataSourceFolder);
                var bulkCopier = new RawDataSourceBulkCopier(dataSourceWriter);
                bulkCopier.CopyFromDataTables(dataTables, rds: rds, progressState: progressState);
                if (!rds.ContainsSourceTableGroup()) {
                    throw new RawDataSourceBulkCopyException("The uploaded database does not contain any recognized source tables.");
                }
            }
            return rds.TableGroups.ToList();
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
                    var reader = new BinaryFolderReader(dataFolder.FullName);
                    reader.Open();
                    var tableDef = McraTableDefinitions.Instance.GetTableDefinition(tableId);
                    var dataReader = reader.GetDataReaderByName(tableName, tableDef);
                    return dataReader;
                }
                return null;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        public (string Name, string Size) GetDatabaseInfo() {
            throw new NotImplementedException();
        }

        public IDictionary<string, string> GetTableInfo() {
            throw new NotImplementedException();
        }
    }
}
