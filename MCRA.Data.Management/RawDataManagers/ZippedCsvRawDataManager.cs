using MCRA.Data.Raw;
using MCRA.Data.Raw.Constants;
using MCRA.General;
using MCRA.General.TableDefinitions;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.DataSourceReading.DataSourceReaders;
using MCRA.Utils.ProgressReporting;
using System.Data;

namespace MCRA.Data.Management.RawDataManagers {
    public sealed class ZippedCsvRawDataManager : IRawDataManager, IDisposable {

        private readonly IDataSourceReader _zipReader;

        public ZippedCsvRawDataManager(string zipFilename) {
            _zipReader = new ZipCsvFileReader(zipFilename);
        }

        public ZippedCsvRawDataManager(Func<IDataSourceReader> dataSourceReaderFactory) {
            _zipReader = dataSourceReaderFactory.Invoke();
        }

        /// <summary>
        /// Implements <see cref="IRawDataManager.CheckRawDataSourceAvailable(int)"/>.
        /// </summary>
        /// <param name="idRawDataSource"></param>
        /// <returns></returns>
        public bool CheckRawDataSourceAvailable(int idRawDataSource) {
            return true;
        }

        /// <summary>
        /// Implements <see cref="IRawDataManager.OpenDataReader{T}(int, out int[])"/>.
        /// </summary>
        /// <param name="idRawDataSource"></param>
        /// <returns></returns>
        public IDataReader OpenDataReader<T>(int idRawDataSource, out int[] fieldMap) where T : IConvertible {
            var tableId = RawTableIdToFieldEnums.EnumToIdMap[typeof(T)];
            return OpenDataReader(idRawDataSource, tableId, out fieldMap);
        }

        /// <summary>
        /// Implements <see cref="IRawDataManager.OpenDataReader(int, RawDataSourceTableID, out int[])"/>.
        /// </summary>
        /// <param name="idRawDataSource"></param>
        /// <param name="tableId"></param>
        /// <param name="fieldMap"></param>
        /// <returns></returns>
        public IDataReader OpenDataReader(int idRawDataSource, RawDataSourceTableID tableId, out int[] fieldMap) {
            if (tableId == RawDataSourceTableID.Unknown) {
                fieldMap = null;
                return null;
            }

            var tableDef = McraTableDefinitions.Instance.GetTableDefinition(tableId);

            // Get the zip file reader
            _zipReader.Open();

            // Get the csv file reader from the zip file
            var reader = _zipReader.GetDataReaderByDefinition(tableDef, out string _);
            if (reader == null) {
                fieldMap = null;
                return null;
            }
            var dataReader = new VirtualStringColumnDataReader(
                reader,
                "idRawDataSource",
                idRawDataSource.ToString()
            );

            fieldMap = tableDef.ColumnDefinitions.GetColumnMappings(dataReader.GetColumnNames());

            return dataReader;
        }

        /// <summary>
        /// Implements <see cref="IRawDataManager.OpenKeysReader(int, RawDataSourceTableID, RawDataSourceTableID[])"/>.
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
            if (McraTableDefinitions.Instance.TableFieldsMap.TryGetValue(idRawTable, out TableFieldsMap tableFieldsMap)) {
                var tableDef = McraTableDefinitions.Instance.GetTableDefinition(idRawTable);
                var columnMappings = new List<int>();

                var idField = tableDef.GetPrimaryKeyColumn();
                var nameField = tableDef.ColumnDefinitions.FirstOrDefault(r => r.IsNameColumn);
                columnMappings.Add(idField != null ? tableDef.GetIndexOfColumnDefinitionByAlias(idField.Id) : -1);
                columnMappings.Add(nameField != null ? tableDef.GetIndexOfColumnDefinitionByAlias(nameField.Id) : -1);
                if (linkedTables?.Any() ?? false) {
                    foreach (var link in linkedTables) {
                        var fieldId = link.KeyField;
                        var columnIndex = tableDef.GetIndexOfColumnDefinitionByAlias(fieldId);
                        columnMappings.Add(columnIndex);
                    }
                }

                // Get the zip file reader
                _zipReader.Open();

                // Get the csv file reader from the zip file
                var reader = _zipReader.GetDataReaderByDefinition(tableDef, out string sourceTableName);
                if (reader == null) {
                    return null;
                }

                var dataReader = new MappedColumnDataReader(reader, columnMappings.ToArray());

                return dataReader;
            }

            return null;
        }

        public (string Name, string Size) GetDatabaseInfo() {
            throw new NotImplementedException();
        }

        public IDictionary<string, string> GetTableInfo() {
            throw new NotImplementedException();
        }

        public ICollection<SourceTableGroup> LoadDataSourceFileIntoDb(IRawDataSourceVersion rds, CompositeProgressState progressState) {
            throw new NotImplementedException();
        }

        public ICollection<SourceTableGroup> LoadDataTablesIntoDb(DataTable[] dataTables, IRawDataSourceVersion rds, CompositeProgressState progressState) {
            throw new NotImplementedException();
        }

        #region IDisposable Members

        public void Dispose() {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        private void dispose(bool disposing) {
            if (_zipReader != null) {
                if (_zipReader is IDisposable) {
                    var disposableItem = (IDisposable)_zipReader;
                    disposableItem.Dispose();
                }
            }
        }
        #endregion
    }
}
