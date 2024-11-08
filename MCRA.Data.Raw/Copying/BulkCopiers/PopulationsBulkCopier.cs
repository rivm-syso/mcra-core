using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Data;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class PopulationsBulkCopier : RawDataSourceBulkCopierBase {

        public PopulationsBulkCopier(
            IDataSourceWriter dataSourceWriter,
            HashSet<SourceTableGroup> parsedTableGroups,
            HashSet<RawDataSourceTableID> parsedDataTables
        ) : base(
            dataSourceWriter,
            parsedTableGroups,
            parsedDataTables
        ) {
        }

        public override void TryCopy(IDataSourceReader dataSourceReader, ProgressState progressState) {
            if (!_parsedDataTables.Contains(RawDataSourceTableID.Populations)) {
                progressState.Update("Processing populations");
                var hasPopulations = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.Populations);
                if (hasPopulations) {
                    var hasIndividualProperties = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.IndividualProperties);
                    if (hasIndividualProperties) {
                        var hasPopulationIndividualPropertyValues = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.PopulationIndividualPropertyValues);
                        if (hasPopulationIndividualPropertyValues) {
                            //Three tables Populations, IndividualProperties, PopulationsIndividualPropertyValues, not recommended
                            tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.Populations);
                        } else {
                            //Two table Populations, IndividualProperties, recommended
                            tryDoBulkCopyWithDynamicPropertyValues(
                                dataSourceReader,
                                RawDataSourceTableID.Populations,
                                RawDataSourceTableID.IndividualProperties,
                                RawDataSourceTableID.PopulationIndividualPropertyValues
                            );
                        }
                    }
                    registerTableGroup(SourceTableGroup.Populations);
                }
            }
            progressState.Update(100);
        }

        /// <summary>
        /// Copies all dynamic population properties based on conventions for numerical and datetime values. Only properties in the IndividualProperties table are copied
        /// </summary>
        /// <param name="dataSourceReader"></param>
        /// <param name="populationSourceTableID"></param>
        /// <param name="individualPropertiesSourceTableID"></param>
        /// <param name="populationIndividualPropertyValuesSourceTableID"></param>
        /// <returns></returns>
        private bool tryDoBulkCopyWithDynamicPropertyValues(
            IDataSourceReader dataSourceReader,
            RawDataSourceTableID populationSourceTableID,
            RawDataSourceTableID individualPropertiesSourceTableID,
            RawDataSourceTableID populationIndividualPropertyValuesSourceTableID
        ) {
            // Check dynamic property values based on availability in individual property table
            if (_parsedDataTables.Contains(populationIndividualPropertyValuesSourceTableID)) {
                return true;
            }

            string sourceTableNamePopulation = null;
            string sourceTableNameIndividualProperties = null;
            try {
                var tableDefinitionPopulation = _tableDefinitions[populationSourceTableID];
                var tableDefinitionPropertyValues = _tableDefinitions[populationIndividualPropertyValuesSourceTableID];
                var tableDefinitionIndividualProperties = _tableDefinitions[individualPropertiesSourceTableID];

                // Get individualProperties, note all checks for the IndividualProperties table are done before in the standard 'tryDoSimpleBulkCopy' action
                var individualProperties = new List<IndividualPropertyTmp>();
                using (var tableReaderIndividualProperties = dataSourceReader
                    .GetDataReaderByDefinition(tableDefinitionIndividualProperties, out sourceTableNameIndividualProperties)
                ) {
                    if (tableReaderIndividualProperties == null) {
                        return false;
                    }
                    dataSourceReader.ValidateSourceTableColumns(tableDefinitionIndividualProperties, tableReaderIndividualProperties);
                    var headers = tableReaderIndividualProperties.GetColumnNames();
                    Func<string, string> findHeader = (columnId) => {
                        var colDef = tableDefinitionIndividualProperties.ColumnDefinitions.First(r => r.Id == columnId);
                        return headers.FirstOrDefault(r => colDef.AcceptsHeader(r, true)) ?? colDef.DefaultValue;
                    };
                    var colNameIdIndividualProperty = findHeader("idIndividualProperty");
                    var colNameType = findHeader("Type");
                    var colNamePropertyLevel = findHeader("PropertyLevel");
                    while (tableReaderIndividualProperties.Read()) {
                        findHeader("idIndividualProperty");
                        individualProperties.Add(new IndividualPropertyTmp() {
                            IdIndividualProperty = Convert.ToString(tableReaderIndividualProperties[colNameIdIndividualProperty]),
                            Type = Convert.ToString(tableReaderIndividualProperties[colNameType]),
                            PropertyLevel = Convert.ToString(tableReaderIndividualProperties[colNamePropertyLevel])
                        });
                    }
                }

                List<string> columnNames;
                using (var sourceTableReaderPopulation = dataSourceReader
                    .GetDataReaderByDefinition(tableDefinitionPopulation, out sourceTableNamePopulation)
                ) {
                    if (sourceTableReaderPopulation == null) {
                        return false;
                    }
                    dataSourceReader.ValidateSourceTableColumns(tableDefinitionPopulation, sourceTableReaderPopulation);
                    columnNames = sourceTableReaderPopulation.GetColumnNames();
                }

                var unMappedColumns = columnNames
                    .Where(c => {
                        var destinationColumn = tableDefinitionPopulation.FindColumnDefinitionByAlias(c);
                        return destinationColumn == null || destinationColumn.IsDynamic;
                    })
                    .ToList();

                var idColumnIndex = tableDefinitionPopulation
                    .GetPrimaryKeyColumn()
                    .GetMatchingHeaderIndex(columnNames);
                if (idColumnIndex < 0) {
                    throw new Exception("Identifier column not found!");
                }
                var idColumnName = columnNames[idColumnIndex];

                if (unMappedColumns.Any()) {
                    var idColumnDefinition = tableDefinitionPropertyValues.ColumnDefinitions
                        .First(r => r.ForeignKeyTables.Contains(tableDefinitionPopulation.Id)).Id;

                    // Copy property values
                    var propertyValuesTable = new DataTable();
                    propertyValuesTable.Columns.Add(new DataColumn(idColumnDefinition, typeof(string)));
                    propertyValuesTable.Columns.Add(new DataColumn("idIndividualProperty", typeof(string)));
                    propertyValuesTable.Columns.Add(new DataColumn("Value", typeof(string)));
                    propertyValuesTable.Columns.Add(new DataColumn("MinValue", typeof(double)));
                    propertyValuesTable.Columns.Add(new DataColumn("MaxValue", typeof(double)));
                    propertyValuesTable.Columns.Add(new DataColumn("StartDate", typeof(DateTime)));
                    propertyValuesTable.Columns.Add(new DataColumn("EndDate", typeof(DateTime)));
                    using var tableReader = dataSourceReader.GetDataReaderByDefinition(tableDefinitionPopulation, out sourceTableNamePopulation);
                    while (tableReader.Read()) {
                        foreach (var item in individualProperties) {
                            if (unMappedColumns.Contains(item.IdIndividualProperty)
                                || unMappedColumns.Contains($"{item.IdIndividualProperty}Min")
                                || unMappedColumns.Contains($"{item.IdIndividualProperty}Max")
                                || unMappedColumns.Contains($"Start{item.IdIndividualProperty}")
                                || unMappedColumns.Contains($"End{item.IdIndividualProperty}")
                            ) {
                                var dr = propertyValuesTable.NewRow();
                                dr[idColumnDefinition] = tableReader[idColumnName];
                                dr["idIndividualProperty"] = item.IdIndividualProperty;

                                if (item.Type.Equals(IndividualPropertyType.Categorical.ToString(), StringComparison.OrdinalIgnoreCase)
                                    || item.Type.Equals(IndividualPropertyType.Boolean.ToString(), StringComparison.OrdinalIgnoreCase)
                                    || item.Type.Equals(IndividualPropertyType.Month.ToString(), StringComparison.OrdinalIgnoreCase)
                                    || item.Type.Equals(IndividualPropertyType.Gender.ToString(), StringComparison.OrdinalIgnoreCase)
                                    || item.Type.Equals(IndividualPropertyType.Location.ToString(), StringComparison.OrdinalIgnoreCase)
                                ) {
                                    var ix = tableReader.GetOrdinal(item.IdIndividualProperty);
                                    if (ix >= 0 && !tableReader.IsDBNull(ix)) {
                                        var value = tableReader[item.IdIndividualProperty].ToString();
                                        if (!string.IsNullOrEmpty(value)) {
                                            dr["Value"] = value;
                                            propertyValuesTable.Rows.Add(dr);
                                        }
                                    }
                                }

                                //Convention for numerical properties: 'individualPropertyName' + 'Min' or 'Max', e.g. for Age => AgeMin or AgeMax
                                if (item.Type.Equals(IndividualPropertyType.Numeric.ToString(), StringComparison.OrdinalIgnoreCase)
                                    || item.Type.Equals(IndividualPropertyType.Nonnegative.ToString(), StringComparison.OrdinalIgnoreCase)
                                    || item.Type.Equals(IndividualPropertyType.Integer.ToString(), StringComparison.OrdinalIgnoreCase)
                                    || item.Type.Equals(IndividualPropertyType.NonnegativeInteger.ToString(), StringComparison.OrdinalIgnoreCase)
                                ) {
                                    var minIndex = tableReader.GetOrdinal($"{item.IdIndividualProperty}Min");
                                    var minValue = minIndex >= 0 && !tableReader.IsDBNull(minIndex)
                                        ? tableReader.GetDouble(minIndex) : double.NaN;
                                    if (!double.IsNaN(minValue)) {
                                        dr["MinValue"] = minValue;
                                    } else {
                                        dr["MinValue"] = DBNull.Value;
                                    }
                                    var maxIndex = tableReader.GetOrdinal($"{item.IdIndividualProperty}Max");
                                    var maxValue = maxIndex >= 0 && !tableReader.IsDBNull(maxIndex)
                                        ? tableReader.GetDouble(maxIndex) : double.NaN;
                                    if (!double.IsNaN(maxValue)) {
                                        dr["MaxValue"] = maxValue;
                                    } else {
                                        dr["MaxValue"] = DBNull.Value;
                                    }
                                    if (dr["MinValue"] != DBNull.Value || dr["MaxValue"] != DBNull.Value) {
                                        propertyValuesTable.Rows.Add(dr);
                                    }
                                }

                                //convention for datetime properties: 'Start' or 'End' +'individualPropertyName', e.g. for Period => StartPeriod or EndPeriod
                                if (item.Type.Equals(IndividualPropertyType.DateTime.ToString(), StringComparison.OrdinalIgnoreCase)) {
                                    var startDateIndex = tableReader.GetOrdinal($"Start{item.IdIndividualProperty}");
                                    var startDate = startDateIndex >= 0 && !tableReader.IsDBNull(startDateIndex)
                                        ? (DateTime?)tableReader.GetDateTime(startDateIndex) : null;
                                    if (startDate != null) {
                                        dr["StartDate"] = (DateTime)startDate;
                                    } else {
                                        dr["StartDate"] = DBNull.Value;
                                    }

                                    var endDateIndex = tableReader.GetOrdinal($"End{item.IdIndividualProperty}");
                                    var endDate = endDateIndex >= 0 && !tableReader.IsDBNull(endDateIndex)
                                        ? (DateTime?)tableReader.GetDateTime(endDateIndex) : null;
                                    if (endDate != null) {
                                        dr["EndDate"] = (DateTime)endDate;
                                    } else {
                                        dr["EndDate"] = DBNull.Value;
                                    }

                                    if (dr["StartDate"] != DBNull.Value || dr["EndDate"] != DBNull.Value) {
                                        propertyValuesTable.Rows.Add(dr);
                                    }
                                }
                            }
                        }
                    }

                    _dataSourceWriter.Write(propertyValuesTable, tableDefinitionPropertyValues.TargetDataTable, tableDefinitionPropertyValues);
                    _parsedDataTables.Add(populationIndividualPropertyValuesSourceTableID);
                }
                return true;
            } catch (Exception ex) {
                var message = $"An error occurred in table '{sourceTableNamePopulation}': {ex.Message}";
                throw new RawDataSourceBulkCopyException(message, sourceTableNamePopulation);
            }
        }

    }

    /// <summary>
    /// Helper class to collect all individual properties.
    /// </summary>
    class IndividualPropertyTmp {
        public string IdIndividualProperty { get; set; }
        public string Type { get; set; }
        public string PropertyLevel { get; set; }
    }
}
