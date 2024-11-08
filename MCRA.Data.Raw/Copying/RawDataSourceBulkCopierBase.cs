using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using MCRA.General.TableDefinitions;
using System.Data;
using System.Globalization;

namespace MCRA.Data.Raw.Copying {

    public abstract class RawDataSourceBulkCopierBase : IRawDataSourceCopier {

        protected static IDictionary<RawDataSourceTableID, TableDefinition> _tableDefinitions = McraTableDefinitions.Instance.TableDefinitions;

        protected IDataSourceWriter _dataSourceWriter;
        protected HashSet<SourceTableGroup> _parsedTableGroups;
        protected HashSet<RawDataSourceTableID> _parsedDataTables;

        /// <summary>
        /// Creates a new instance for the target database connection.
        /// </summary>
        /// <param name="dataSourceWriter">Connection to the target database.</param>
        /// <param name="parsedTableGroups">Collection of already copied table groups.</param>
        /// <param name="parsedDataTables">Collection of already copied tables.</param>
        public RawDataSourceBulkCopierBase(
            IDataSourceWriter dataSourceWriter,
            HashSet<SourceTableGroup> parsedTableGroups,
            HashSet<RawDataSourceTableID> parsedDataTables
        ) {
            _parsedTableGroups = parsedTableGroups ?? [];
            _parsedDataTables = parsedDataTables ?? [];
            _dataSourceWriter = dataSourceWriter;
        }

        /// <summary>
        /// Runs the bulk copy procedure and returns the parsed table groups as a list.
        /// </summary>
        /// <param name="dataSourceReader"></param>
        /// <param name="progressState"></param>
        /// <returns></returns>
        public ICollection<SourceTableGroup> Copy(
            IDataSourceReader dataSourceReader,
            ProgressState progressState
        ) {
            TryCopy(dataSourceReader, progressState);
            return _parsedTableGroups;
        }

        /// <summary>
        /// Should be implemented by derived classes. Should start the copy operation of the
        /// bulk copier.
        /// </summary>
        /// <param name="progressState"></param>
        public abstract void TryCopy(
            IDataSourceReader dataSourceReader,
            ProgressState progressState
        );

        /// <summary>
        /// Copies all tables from the rawdatasource that are found in the list of table aliases
        /// that correspond to SourceTableID, to a single table in the backend database.
        /// </summary>
        /// <param name="dataSourceReader">The ID of the table that is copied to.</param>
        /// <param name="sourceTableID">The ID of the table that is copied to.</param>
        protected bool tryDoSimpleBulkCopy(
            IDataSourceReader dataSourceReader,
            RawDataSourceTableID sourceTableID
        ) {
            if (_parsedDataTables.Contains(sourceTableID)) {
                // Table was already parsed/copied/added
                return true;
            }

            string sourceTableName = null;
            try {
                var tableDefinition = _tableDefinitions[sourceTableID];
                using var sourceTableReader = dataSourceReader.GetDataReaderByDefinition(tableDefinition, out sourceTableName);
                if (sourceTableReader != null) {
                    dataSourceReader.ValidateSourceTableColumns(tableDefinition, sourceTableReader);
                    _dataSourceWriter.Write(sourceTableReader, tableDefinition, tableDefinition.TargetDataTable);
                    _parsedDataTables.Add(sourceTableID);
                    return true;
                }
                return false;
            } catch (Exception ex) {
                var tableName = sourceTableName.Replace("$", "");
                var message = $"An error occurred in table '{tableName}': {ex.Message}";
                throw new RawDataSourceBulkCopyException(message, tableName);
            }
        }

        /// <summary>
        /// Tries to copy the data from the table reader to the source table
        /// specified by the raw data source table id.
        /// </summary>
        /// <param name="sourceTableReader"></param>
        /// <param name="sourceTableID"></param>
        /// <returns></returns>
        protected bool tryDoSimpleBulkCopy(
            IDataReader sourceTableReader,
            RawDataSourceTableID sourceTableID
        ) {
            string sourceTableName = null;
            try {
                var tableDefinition = _tableDefinitions[sourceTableID];
                if (sourceTableReader != null) {
                    _dataSourceWriter.Write(sourceTableReader, tableDefinition, tableDefinition.TargetDataTable);
                    _parsedDataTables.Add(sourceTableID);
                    return true;
                }
                return false;
            } catch (Exception ex) {
                var tableName = sourceTableName.Replace("$", "");
                var message = $"An error occurred in table '{tableName}': {ex.Message}";
                throw new RawDataSourceBulkCopyException(message, tableName);
            }
        }

        /// <summary>
        /// Tries to copy a data table of the specified table type into the specified destination table.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="sourceTableID"></param>
        /// <returns></returns>
        protected bool tryCopyDataTable(
            DataTable dataTable,
            RawDataSourceTableID sourceTableID
        ) {
            try {
                var tableDefinition = _tableDefinitions[sourceTableID];
                _dataSourceWriter.Write(dataTable, tableDefinition.TargetDataTable, tableDefinition);
                _parsedDataTables.Add(sourceTableID);
                return true;
            } catch (Exception ex) {
                var tableName = sourceTableID.ToString();
                var message = $"An error occurred in table '{tableName}': {ex.Message}";
                throw new RawDataSourceBulkCopyException(message, tableName);
            }
        }

        /// <summary>
        /// Copies all tables from the rawdatasource that are found in the list of table aliases
        /// that correspond to SourceTableID, to a single table in the backend database.
        /// </summary>
        /// <param name="sourceTableID">The ID of the table that is copied to.</param>
        /// <param name="dataSourceReader"> The data source reader</param>
        /// <param name="propertiesSourceTableID">The table id of the destination table for the properties (names)</param>
        /// <param name="propertyValuesSourceTableID">The table id of the destination table for the property values</param>
        protected bool tryDoBulkCopyWithDynamicProperties(
            IDataSourceReader dataSourceReader,
            RawDataSourceTableID sourceTableID,
            RawDataSourceTableID propertiesSourceTableID,
            RawDataSourceTableID propertyValuesSourceTableID
        ) {
            if (_parsedDataTables.Contains(sourceTableID)) {
                return true;
            }
            string sourceTableName = null;
            try {
                var tableDefinition = _tableDefinitions[sourceTableID];
                var propertiesTableDefinition = _tableDefinitions[propertiesSourceTableID];
                var propertyValuesTableDefinition = _tableDefinitions[propertyValuesSourceTableID];
                using var sourceTableReader = dataSourceReader.GetDataReaderByDefinition(
                    tableDefinition,
                    out sourceTableName
                );
                if (sourceTableReader == null) {
                    return false;
                }

                dataSourceReader.ValidateSourceTableColumns(tableDefinition, sourceTableReader);

                if (sourceTableReader != null) {
                    //Never copy "IdRawDataSource" field
                    var columnNames = sourceTableReader
                        .GetColumnNames()
                        .Where(n => n.ToLower() != "idrawdatasource")
                        .ToList();

                    _dataSourceWriter.Write(sourceTableReader, tableDefinition, tableDefinition.TargetDataTable);
                    _parsedDataTables.Add(sourceTableID);

                    var idProperty = tableDefinition.GetPrimaryKeyColumn();
                    var targetPropertyKey = idProperty.Id;

                    var unmappedColumns = new List<string>();
                    foreach (var name in columnNames) {
                        var destColumn = tableDefinition.FindColumnDefinitionByAlias(name);
                        if (destColumn == null || destColumn.IsDynamic) {
                            unmappedColumns.Add(name);
                        }
                    }

                    var idColumnIndex = idProperty.GetMatchingHeaderIndex(columnNames);
                    if (idColumnIndex < 0) {
                        throw new Exception("Identifier column not found!");
                    }
                    var idColumnName = columnNames[idColumnIndex];

                    // Copy dynamic property definitions
                    if (unmappedColumns.Any()) {
                        var propertyTable = propertiesTableDefinition.CreateDataTable();
                        var primaryKey = propertiesTableDefinition.ColumnDefinitions.FirstOrDefault(r => r.IsPrimaryKey);

                        foreach (var c in unmappedColumns) {
                            var dr = propertyTable.NewRow();
                            dr[primaryKey.Id] = c;
                            propertyTable.Rows.Add(dr);
                        }
                        _dataSourceWriter.Write(propertyTable, propertiesTableDefinition.TargetDataTable, propertiesTableDefinition);
                        _parsedDataTables.Add(propertiesSourceTableID);

                        var idColumnDefinition = propertyValuesTableDefinition.ColumnDefinitions
                            .First(r => r.ForeignKeyTables.Contains(tableDefinition.Id)).Id;

                        // Copy dynamic property values
                        var propertyValuesTable = propertyValuesTableDefinition.CreateDataTable();
                        using var tableReader = dataSourceReader.GetDataReaderByDefinition(tableDefinition, out sourceTableName);
                        while (tableReader.Read()) {
                            foreach (var propertyName in unmappedColumns) {
                                var dr = propertyValuesTable.NewRow();
                                dr[idColumnDefinition] = tableReader[idColumnName];
                                dr["PropertyName"] = propertyName;
                                var textValue = Convert.ToString(tableReader[propertyName]);
                                if (double.TryParse(textValue.Replace(',', '.'),
                                                    NumberStyles.Any,
                                                    NumberFormatInfo.InvariantInfo,
                                                    out double doubleValue)
                                ) {
                                    dr["DoubleValue"] = doubleValue;
                                } else {
                                    dr["TextValue"] = textValue;
                                }
                                propertyValuesTable.Rows.Add(dr);
                            }
                        }
                        _dataSourceWriter.Write(propertyValuesTable, propertyValuesTableDefinition.TargetDataTable, propertyValuesTableDefinition);
                        _parsedDataTables.Add(propertyValuesSourceTableID);
                    }

                    sourceTableReader.Close();
                }
                return true;
            } catch (Exception ex) {
                var message = $"An error occurred in table '{sourceTableName}': {ex.Message}";
                throw new RawDataSourceBulkCopyException(message, sourceTableName);
            }
        }

        /// <summary>
        /// Copies all tables from the rawdatasource that are found in the list of table aliases
        /// that correspond to SourceTableID, to a single table in the backend database.
        /// Copies dynamic property values from source table (e.g. Individuals) like breastfeeding
        /// </summary>
        /// <param name="sourceTableID">The ID of the table that is copied to.</param>
        /// <param name="dataSourceReader"> The data source reader</param>
        /// <param name="propertyValuesSourceTableID">The table id of the destination table for the property values</param>
        protected bool tryDoBulkCopyWithDynamicPropertyValues(
            IDataSourceReader dataSourceReader,
            RawDataSourceTableID sourceTableID,
            RawDataSourceTableID propertyValuesSourceTableID
        ) {
            //check dynamic property values based on availability in individual property table
            if (_parsedDataTables.Contains(propertyValuesSourceTableID)) {
                return true;
            }

            string sourceTableName = null;
            try {
                var tableDefinition = _tableDefinitions[sourceTableID];
                var propertyValuesTableDefinition = _tableDefinitions[propertyValuesSourceTableID];
                using var sourceTableReader = dataSourceReader.GetDataReaderByDefinition(
                    tableDefinition,
                    out sourceTableName
                );
                if (sourceTableReader == null) {
                    return false;
                }

                dataSourceReader.ValidateSourceTableColumns(tableDefinition, sourceTableReader);

                if (sourceTableReader != null) {
                    var columnNames = sourceTableReader.GetColumnNames();

                    var idProperty = tableDefinition.GetPrimaryKeyColumn();
                    var targetPropertyKey = idProperty.Id;

                    var unMappedColumns = columnNames
                        .Where(c => {
                            var destinationColumn = tableDefinition.FindColumnDefinitionByAlias(c);
                            return destinationColumn == null || destinationColumn.IsDynamic;
                        })
                        .ToList();

                    var idColumnIndex = idProperty.GetMatchingHeaderIndex(columnNames);
                    if (idColumnIndex < 0) {
                        throw new Exception("Identifier column not found!");
                    }
                    var idColumnName = columnNames[idColumnIndex];

                    if (unMappedColumns.Any()) {
                        var idColumnDefinition = propertyValuesTableDefinition.ColumnDefinitions
                            .First(r => r.ForeignKeyTables.Contains(tableDefinition.Id)).Id;

                        // Copy property values
                        var propertyValuesTable = new DataTable();
                        propertyValuesTable.Columns.Add(new DataColumn(idColumnDefinition, typeof(string)));
                        propertyValuesTable.Columns.Add(new DataColumn("PropertyName", typeof(string)));
                        propertyValuesTable.Columns.Add(new DataColumn("TextValue", typeof(string)));
                        propertyValuesTable.Columns.Add(new DataColumn("DoubleValue", typeof(double)));
                        using var tableReader = dataSourceReader.GetDataReaderByDefinition(tableDefinition, out sourceTableName);
                        while (tableReader.Read()) {
                            foreach (var propertyName in unMappedColumns) {
                                var dr = propertyValuesTable.NewRow();
                                dr[idColumnDefinition] = tableReader[idColumnName];
                                dr["PropertyName"] = propertyName;
                                var textValue = Convert.ToString(tableReader[propertyName]);
                                if (double.TryParse(textValue.Replace(',', '.'),
                                                    NumberStyles.Any,
                                                    NumberFormatInfo.InvariantInfo,
                                                    out double doubleValue)
                                ) {
                                    dr["DoubleValue"] = doubleValue;
                                } else {
                                    dr["TextValue"] = textValue;
                                }
                                propertyValuesTable.Rows.Add(dr);
                            }
                        }
                        _dataSourceWriter.Write(propertyValuesTable, propertyValuesTableDefinition.TargetDataTable, propertyValuesTableDefinition);
                        _parsedDataTables.Add(propertyValuesSourceTableID);
                    }

                    sourceTableReader.Close();
                }
                return true;
            } catch (Exception ex) {
                var message = $"An error occurred in table '{sourceTableName}': {ex.Message}";
                throw new RawDataSourceBulkCopyException(message, sourceTableName);
            }
        }

        /// <summary>
        /// Flags the table group as being read by the bulk copier.
        /// </summary>
        /// <param name="tableGroup"></param>
        protected void registerTableGroup(SourceTableGroup tableGroup) {
            _parsedTableGroups.Add(tableGroup);
        }

        /// <summary>
        /// Flags the data table as being read by the bulk copier.
        /// </summary>
        /// <param name="rawDataSourceTableID"></param>
        protected void registerDataTable(RawDataSourceTableID rawDataSourceTableID) {
            _parsedDataTables.Add(rawDataSourceTableID);
        }
    }
}
