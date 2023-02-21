using MCRA.Data.Raw;
using MCRA.Data.Raw.Constants;
using MCRA.General;
using MCRA.General.ScopingTypeDefinitions;
using MCRA.General.TableDefinitions;
using MCRA.Utils.Collections;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MCRA.Data.Management.RawDataManagers {
    public class CsvTableRawDataManager : IRawDataManager {

        private readonly string _csvBasePath;
        private readonly Dictionary<(int RawDsId, RawDataSourceTableID TableId), DataTable> _dataTables;

        public CsvTableRawDataManager(
            string csvBaseFilePath
        ) {
            _csvBasePath = csvBaseFilePath;
            _dataTables = new Dictionary<(int, RawDataSourceTableID), DataTable>();
        }

        /// <summary>
        /// Sets the specified csv file as source for the specified scoping type.
        /// </summary>
        /// <param name="tableId"></param>
        /// <param name="csvResourceName"></param>
        /// <param name="idRawDataSource"></param>
        /// <returns></returns>
        public int SetDataTable(RawDataSourceTableID tableId, string csvResourceName, int idRawDataSource = -1) {
            SetDataTable(tableId, idRawDataSource, csvResourceName);
            return idRawDataSource;
        }

        /// <summary>
        /// Data source from folder: use one rawdatasource id for all content in the folder.
        /// </summary>
        /// <param name="idRawDataSource"></param>
        /// <param name="csvResourceFolder"></param>
        /// <param name="tableGroups"></param>
        public void SetDataTablesFromFolder(int idRawDataSource, string csvResourceFolder, params SourceTableGroup[] tableGroups) {
            foreach (var tableGroup in tableGroups) {
                var scopingTypes = McraScopingTypeDefinitions.Instance.TableGroupScopingTypesLookup[tableGroup];
                foreach (var val in scopingTypes) {
                    var resourceName = $@"{csvResourceFolder}\{val.Id}";
                    var csvFilePath = Path.Combine(_csvBasePath, $@"{resourceName}.csv");
                    if (val.RawTableId != null && File.Exists(csvFilePath)) {
                        SetDataTable((RawDataSourceTableID)val.RawTableId, resourceName, idRawDataSource);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the specified csv file as raw data source for the specified table and id raw data source.
        /// </summary>
        /// <param name="tableId"></param>
        /// <param name="idRawDataSource"></param>
        /// <param name="csvResourceName"></param>
        public void SetDataTable(RawDataSourceTableID tableId, int idRawDataSource, string csvResourceName) {
            var table = createDataTable(tableId, csvResourceName);
            _dataTables.Add((idRawDataSource, tableId), table);
        }

        /// <summary>
        /// Checks whether there is a data source with the specified id.
        /// </summary>
        /// <param name="idRawDataSource"></param>
        /// <returns></returns>
        public bool CheckRawDataSourceAvailable(int idRawDataSource) {
            return _dataTables.Keys.Any(r => r.RawDsId == idRawDataSource);
        }

        /// <summary>
        /// Implements <see cref="IRawDataManager.OpenDataReader{T}(int, out int[])"/>.
        /// Returns a data reader for the raw data source with the specified id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idRawDataSource"></param>
        /// <param name="fieldMap"></param>
        /// <returns></returns>
        public IDataReader OpenDataReader<T>(int idRawDataSource, out int[] fieldMap) where T : IConvertible {
            var tableId = RawTableIdToFieldEnums.EnumToIdMap[typeof(T)];
            return OpenDataReader(idRawDataSource, tableId, out fieldMap);
        }

        /// <summary>
        /// Implements <see cref="IRawDataManager.OpenDataReader(int, RawDataSourceTableID, out int[])"/>.
        /// Opens a data reader for the specified raw data source and table.
        /// </summary>
        /// <param name="idRawDataSource"></param>
        /// <param name="tableId"></param>
        /// <param name="fieldMap"></param>
        /// <returns></returns>
        public IDataReader OpenDataReader(int idRawDataSource, RawDataSourceTableID tableId, out int[] fieldMap) {
            var resolvedReader = getDataTableReader(idRawDataSource, tableId);
            if (resolvedReader != null) {
                var tableDef = McraTableDefinitions.Instance.GetTableDefinition(tableId);
                fieldMap = tableDef.ColumnDefinitions.GetColumnMappings(resolvedReader.GetColumnNames());
            } else {
                fieldMap = null;
            }
            return resolvedReader;
        }

        /// <summary>
        /// Implements <see cref="IRawDataManager.OpenKeysReader(int, RawDataSourceTableID, RawDataSourceTableID[])"/>.
        /// Opens a keys reader for the specified data source and table type containing the primary key, name,
        /// and the foreign key references.
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
            if (McraTableDefinitions.Instance.TableFieldsMap.TryGetValue(idRawTable, out TableFieldsMap dto)) {

                if (_dataTables.TryGetValue((idRawDataSource, idRawTable), out var rawTable)) {
                    var primaryKeyFieldName = dto.PrimaryKeyField?.ToString();
                    var nameFieldName = dto.NameField?.ToString();

                    var dt = new DataTable();
                    DataColumn[] dataColumns = new DataColumn[2 + linkedTables.Length];

                    dataColumns[0] = dt.Columns.Add("Keys", typeof(string));
                    dataColumns[1] = dt.Columns.Add("Name", typeof(string));

                    for (int i = 0; i < linkedTables.Length; i++) {
                        dataColumns[i + 2] = dt.Columns.Add($"Parent{i}", typeof(string));
                    }

                    foreach (DataRow row in rawTable.Rows) {
                        var r = dt.NewRow();
                        r[0] = primaryKeyFieldName != null ? row[primaryKeyFieldName].ToString() : null;
                        r[1] = nameFieldName != null ? row[nameFieldName].ToString() : null;
                        for (int i = 0; i < linkedTables.Length; i++) {
                            r[i + 2] = row[linkedTables[i].Item2].ToString();
                        }
                        try {
                            dt.Rows.Add(r);
                        } catch (ConstraintException) {
                            // Only add unique rows, don't do anything
                            // when this exception occurs
                            // TODO (performance): catching errors is probably not the most
                            // efficient way to get the unique rows
                        }
                    }

                    return dt.CreateDataReader();
                }
            }
            return new DataTable().CreateDataReader();
        }

        private IDataReader getDataTableReader(int idRawDataSource, RawDataSourceTableID tableId) {
            return _dataTables.TryGetValue((idRawDataSource, tableId), out var dataTable)
                 ? dataTable.CreateDataReader()
                 : null;
        }

        private DataTable createDataTable(RawDataSourceTableID tableId, string csvResourceName = null) {
            var tableDef = McraTableDefinitions.Instance.TableDefinitions[tableId];
            var table = tableDef.CreateDataTable();
            if (!string.IsNullOrWhiteSpace(csvResourceName)) {
                //create CSV reader with tablecontents as (text) stream
                //first create stream from the CSV resource
                var path = Path.Combine(_csvBasePath, $@"{csvResourceName}.csv");
                using (var reader = new StreamReader(path)) {
                    var stream = reader.BaseStream;
                    var dataReader = new TableDefinitionDataReader(
                        new CsvDataReader(stream),
                        tableDef,
                        useDefinitionColumnNames: true
                    );
                    table.Load(dataReader, LoadOption.OverwriteChanges);
                    Debug.WriteLine($"Table {tableId} loaded");
                }
            }
            return table;
        }

        public ICollection<SourceTableGroup> LoadDataSourceFileIntoDb(IRawDataSourceVersion rds, CompositeProgressState progressState) {
            throw new NotImplementedException();
        }

        public ICollection<SourceTableGroup> LoadDataTablesIntoDb(DataTable[] dataTables, IRawDataSourceVersion rds, CompositeProgressState progressState) {
            throw new NotImplementedException();
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
