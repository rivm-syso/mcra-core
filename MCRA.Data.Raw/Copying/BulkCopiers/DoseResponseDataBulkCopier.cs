using System.Data;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using MCRA.Data.Raw.Copying.BulkCopiers.DoseResponse;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class DoseResponseDataBulkCopier : RawDataSourceBulkCopierBase {

        public DoseResponseDataBulkCopier(
            IDataSourceWriter dataSourceWriter,
            HashSet<SourceTableGroup> parsedTableGroups,
            HashSet<RawDataSourceTableID> parsedDataTables)
            : base(
                  dataSourceWriter,
                  parsedTableGroups,
                  parsedDataTables
        ) {
        }

        public override void TryCopy(IDataSourceReader dataFileReader, ProgressState progressState) {
            var hasDoseResponseExperiments = tryDoSimpleBulkCopy(dataFileReader, RawDataSourceTableID.DoseResponseExperiments);
            if (hasDoseResponseExperiments) {
                var hasRelationalDataTables = tryCopyRelationalTables(dataFileReader);
                if (!hasRelationalDataTables) {
                    var experiments = readExperiments(dataFileReader);
                    var hasDoseResponseData = tryCopyTwoWayDoseResponseDataTableBulkCopy(dataFileReader, experiments);
                    if (!hasDoseResponseData) {
                        var message = string.Format("Failed to process dose response experiment data; cannot find dose-response data.");
                        throw new RawDataSourceBulkCopyException(message);
                    }
                }
                registerTableGroup(SourceTableGroup.DoseResponseData);
            }
        }

        #region Relational table bulk copy

        private bool tryCopyRelationalTables(IDataSourceReader dataFileReader) {
            var hasDoses = tryDoSimpleBulkCopy(dataFileReader, RawDataSourceTableID.DoseResponseExperimentDoses);
            var hasMeasurements = tryDoSimpleBulkCopy(dataFileReader, RawDataSourceTableID.DoseResponseExperimentMeasurements);
            tryDoBulkCopyDoseResponseExperimentProperties(dataFileReader, RawDataSourceTableID.ExperimentalUnitProperties);
            if (hasDoses && hasMeasurements) {
                return true;
            }
            return false;
        }

        private bool tryDoBulkCopyDoseResponseExperimentProperties(IDataSourceReader dataFileReader, RawDataSourceTableID sourceTableID) {
            string sourceTableName = null;
            try {
                var tableDefinition = _tableDefinitions[sourceTableID];
                using var sourceTableReader = dataFileReader.GetDataReaderByDefinition(tableDefinition, out sourceTableName);
                if (sourceTableReader == null) {
                    return false;
                }

                dataFileReader.ValidateSourceTableColumns(tableDefinition, sourceTableReader);

                if (sourceTableReader != null) {
                    var columnNames = sourceTableReader.GetColumnNames();

                    var mappedColumns = columnNames
                        .Select(c => new {
                            SourceColumn = c,
                            DestinationColumn = tableDefinition.FindColumnDefinitionByAlias(c)
                        })
                        .Where(r => r.DestinationColumn != null)
                        .ToList();

                    var unMappedColumns = columnNames
                        .Where(c => {
                            var destinationColumn = tableDefinition.FindColumnDefinitionByAlias(c);
                            return destinationColumn == null || destinationColumn.IsDynamic;
                        })
                        .ToList();

                    if (unMappedColumns.Any()) {
                        // Create properties data table
                        var propertyValuesTable = new DataTable();
                        foreach (var mappedColumn in mappedColumns) {
                            propertyValuesTable.Columns.Add(new DataColumn(mappedColumn.DestinationColumn.Id, typeof(string)));
                        }
                        propertyValuesTable.Columns.Add(new DataColumn("PropertyName", typeof(string)));
                        propertyValuesTable.Columns.Add(new DataColumn("Value", typeof(string)));

                        using var tableReader = dataFileReader.GetDataReaderByDefinition(tableDefinition, out sourceTableName);

                        while (tableReader.Read()) {
                            foreach (var propertyName in unMappedColumns) {
                                var dr = propertyValuesTable.NewRow();
                                foreach (var mappedColumn in mappedColumns) {
                                    dr[mappedColumn.DestinationColumn.Id] = tableReader[mappedColumn.SourceColumn];
                                }
                                dr["PropertyName"] = propertyName;
                                var textValue = Convert.ToString(tableReader[propertyName]);
                                dr["Value"] = textValue;
                                propertyValuesTable.Rows.Add(dr);
                            }
                        }

                        tryCopyDataTable(propertyValuesTable, RawDataSourceTableID.ExperimentalUnitProperties);
                    }

                    sourceTableReader.Close();

                    return true;
                }
                return false;
            } catch (Exception ex) {
                var defaultMessage = $"An error occurred in table '{sourceTableName}': {ex.Message}";
                throw new RawDataSourceBulkCopyException(defaultMessage, sourceTableName);
            }
        }

        #endregion

        #region Two-way table bulk copy

        private List<DoseResponseExperimentRecord> readExperiments(IDataSourceReader dataSourceReader) {
            string sourceTableName = null;
            try {
                var tableDefinition = _tableDefinitions[RawDataSourceTableID.DoseResponseExperiments];
                using var sourceTableReader = dataSourceReader.GetDataReaderByDefinition(tableDefinition, out sourceTableName);
                if (sourceTableReader == null) {
                    return null;
                }
                dataSourceReader.ValidateSourceTableColumns(tableDefinition, sourceTableReader);
                var records = dataSourceReader.ReadDataTable<DoseResponseExperimentRecord>(tableDefinition)
                    .Where(r => !string.IsNullOrEmpty(r.idExperiment)).ToList();
                return records;
            } catch (Exception ex) {
                var defaultMessage = $"An error occurred in table '{sourceTableName}': {ex.Message}";
                throw new RawDataSourceBulkCopyException(defaultMessage, sourceTableName);
            }
        }

        private bool tryCopyTwoWayDoseResponseDataTableBulkCopy(
            IDataSourceReader dataFileReader, 
            List<DoseResponseExperimentRecord> experiments
        ) {
            string sourceTableName = null;
            try {
                var tableDefinition = _tableDefinitions[RawDataSourceTableID.DoseResponseData];

                // Try get dose response data reader in sheet DoseResponseData
                using var sourceTableReader = dataFileReader.GetDataReaderByDefinition(tableDefinition, out sourceTableName);
                if (sourceTableReader != null) {
                    //Read dose response data from common DoseResponseData sheet
                    foreach (var experiment in experiments) {
                        if (!writeDoseExperimentData(dataFileReader, experiment, tableDefinition, sourceTableReader)) {
                            return false;
                        }
                    }
                } else {
                    // Read dose response data with sheetNames specified in DoseResponseExperiments
                    foreach (var experiment in experiments) {
                        var sourceTableReaderByName = dataFileReader.GetDataReaderByName(experiment.idExperiment, tableDefinition);
                        if (sourceTableReaderByName == null) {
                            throw new RawDataSourceBulkCopyException($" dose response experiments: experiment {experiment.idExperiment} is specified but not found.");
                        }
                        if (!writeDoseExperimentData(dataFileReader, experiment, tableDefinition, sourceTableReaderByName)) {
                            return false;
                        }
                    }
                }
                return true;
            } catch (Exception ex) {
                var defaultMessage = $"An error occurred while copying the dose response data: {ex.Message}";
                throw new RawDataSourceBulkCopyException(defaultMessage, sourceTableName);
            }
        }

        private bool writeDoseExperimentData(
            IDataSourceReader dataSourceReader,
            DoseResponseExperimentRecord experiment,
            TableDefinition tableDefinition,
            IDataReader sourceTableReader
        ) {
            dataSourceReader.ValidateSourceTableColumns(tableDefinition, sourceTableReader);

            // Read dose response data
            var doseResponseData = readDoseResponseData(experiment, tableDefinition, sourceTableReader);

            // Create the dose response data tables
            var dosesTable = doseResponseData.DoseResponseDoseRecords.ToDataTable();
            var measurementsTable = doseResponseData.DoseResponseMeasurementRecords.ToDataTable();
            var propertiesTable = doseResponseData.ExperimentalUnitProperties.ToDataTable();

            // Write data tables to database
            tryCopyDataTable(dosesTable, RawDataSourceTableID.DoseResponseExperimentDoses);
            tryCopyDataTable(measurementsTable, RawDataSourceTableID.DoseResponseExperimentMeasurements);
            tryCopyDataTable(propertiesTable, RawDataSourceTableID.ExperimentalUnitProperties);

            return true;
        }

        /// <summary>
        /// Responses are specified as "N:response", "SD:response" but occasionally a space is added. 
        /// Here these trailing spaces are removed from the 2e element of the list (to be sure), because people specify in general a bunch of crab.
        /// </summary>
        /// <param name="fieldname"></param>
        /// <returns></returns>
        private string strip(string fieldname) {
            if (fieldname.Contains(":")) {
                var names = fieldname.Split(':').ToList();
                names[1] = names[1].TrimStart();
                fieldname = string.Join(":", names);
            }
            return fieldname;
        }

        private DoseResponseData readDoseResponseData(
            DoseResponseExperimentRecord experiment,
            TableDefinition tableDefinition,
            IDataReader sourceTableReader
        ) {
            var columnIndexes = new Dictionary<string, int>();
            for (int i = 0; i < sourceTableReader.FieldCount; i++) {
                var fieldName = strip(sourceTableReader.GetName(i).Trim());
                if (!string.IsNullOrEmpty(fieldName)) {
                    if (columnIndexes.ContainsKey(fieldName)) {
                        throw new Exception($"Error in dose response data for experiment {experiment.idExperiment}: duplicate column header '{fieldName}'");
                    }
                    columnIndexes.Add(fieldName, i);
                }
            }

            var mappings = getDoseResponseDataFieldMappings(experiment, columnIndexes);

            var experimentFieldDefinition = tableDefinition.FindColumnDefinitionByAlias("experiment");
            var experimentField = -1;
            var experimentFieldHeader = columnIndexes.FirstOrDefault(r => experimentFieldDefinition.AcceptsHeader(r.Key, true));
            if (experimentFieldHeader.Key != null) {
                experimentField = experimentFieldHeader.Value;
            }

            var doseResponseExperimentDoses = new List<DoseResponseDoseRecord>();
            var doseResponseExperimentMeasurements = new List<DoseResponseMeasurementRecord>();
            var experimentalUnitProperties = new Dictionary<(string, string), List<ExperimentalUnitPropertyRecord>>();
            var idExperiment = experiment.idExperiment;
            var experimentalUnitCounter = 0;
            while (sourceTableReader.Read()) {
                if (experimentField > -1) {
                    idExperiment = sourceTableReader.GetString(experimentField);
                }

                if (!mappings.ContainsKey(idExperiment)) {
                    throw new Exception($"Failed to match dose response data record with experiment code {experiment.idExperiment}");
                }
                var experimentMappings = mappings[idExperiment];
                var experimentalUnit = string.Join(":", experimentMappings.ExperimentalUnitMappings.Select(c => Convert.ToString(sourceTableReader.GetValue(c.Value))));
                if (!experimentalUnitProperties.ContainsKey((idExperiment, experimentalUnit)) && !string.IsNullOrEmpty(experimentalUnit)) {
                    var propertyValues = new List<ExperimentalUnitPropertyRecord>();
                    foreach (var covariateMapping in experimentMappings.CovariateMappings) {
                        var propertyValue = new ExperimentalUnitPropertyRecord() {
                            IdExperiment = idExperiment,
                            IdExperimentalUnit = experimentalUnit,
                            PropertyName = covariateMapping.Key,
                            Value = Convert.ToString(sourceTableReader.GetValue(covariateMapping.Value))
                        };
                        propertyValues.Add(propertyValue);
                    }
                    var blockingFactor = experiment.ExperimentalUnit.Split(':').First();
                    propertyValues.Add(new ExperimentalUnitPropertyRecord() {
                        IdExperiment = idExperiment,
                        IdExperimentalUnit = experimentalUnit,
                        PropertyName = blockingFactor,
                        Value = experimentMappings.ExperimentalUnitMappings.Select(c => Convert.ToString(sourceTableReader.GetValue(c.Value))).First(),
                    });
                    experimentalUnitProperties.Add((idExperiment, experimentalUnit), propertyValues);
                } else if (string.IsNullOrEmpty(experiment.ExperimentalUnit)) {
                    experimentalUnit = experimentalUnitCounter.ToString();
                }

                var time = GetFloatOrNull(sourceTableReader, experimentMappings.TimeField);
                foreach (var substance in experimentMappings.SubstanceMappings) {
                    var dose = GetDoubleOrNull(sourceTableReader, substance.Value);
                    if (dose.HasValue) {
                        var doseRecord = new DoseResponseDoseRecord() {
                            IdExperiment = idExperiment,
                            IdExperimentalUnit = experimentalUnit,
                            Time = time,
                            IdSubstance = substance.Key,
                            Dose = dose.Value,
                        };
                        doseResponseExperimentDoses.Add(doseRecord);
                    }
                }

                foreach (var response in experimentMappings.ResponseMappings) {
                    var responseValue = GetDoubleOrNull(sourceTableReader, response.Value.ColumnValue);
                    if (responseValue.HasValue) {
                        var responseRecord = new DoseResponseMeasurementRecord {
                            IdExperiment = idExperiment,
                            IdExperimentalUnit = experimentalUnit,
                            Time = time,
                            IdResponse = response.Key,
                            ResponseValue = responseValue.Value,
                            ResponseCV = GetDoubleOrNull(sourceTableReader, response.Value.ColumnCv),
                            ResponseN = GetDoubleOrNull(sourceTableReader, response.Value.ColumnN),
                            ResponseSD = GetDoubleOrNull(sourceTableReader, response.Value.ColumnSd),
                            ResponseUncertaintyUpper = GetDoubleOrNull(sourceTableReader, response.Value.ColumnUncertainty),
                        };
                        doseResponseExperimentMeasurements.Add(responseRecord);
                    }
                }
                experimentalUnitCounter++;
            }

            var doseResponseData = new DoseResponseData() {
                DoseResponseDoseRecords = doseResponseExperimentDoses,
                DoseResponseMeasurementRecords = doseResponseExperimentMeasurements,
                ExperimentalUnitProperties = experimentalUnitProperties.Values.SelectMany(r => r).ToList(),
            };
            return doseResponseData;
        }

        private Dictionary<string, DoseResponseExperimentMapping> getDoseResponseDataFieldMappings(DoseResponseExperimentRecord experiment, Dictionary<string, int> columnIndexes) {
            var responseMappings = new Dictionary<string, DoseResponseExperimentMapping>(StringComparer.OrdinalIgnoreCase);
            var mapping = new DoseResponseExperimentMapping();

            // Experimental unit mapping
            var experimentalUnits = experiment.ExperimentalUnit?.Split(':')?.Where(r => !string.IsNullOrEmpty(r)) ?? new List<string>();

            //Experimentalunit mappings
            mapping.ExperimentalUnitMappings = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in experimentalUnits) {
                if (!columnIndexes.ContainsKey(item)) {
                    throw new Exception($"Cannot find experimental unit field {experiment.ExperimentalUnit} for experiment {experiment.idExperiment}");
                }
                mapping.ExperimentalUnitMappings.Add(item, columnIndexes[item]);
            }

            // Time mapping
            var test = string.IsNullOrEmpty(experiment.Time);
            if (experiment.Time != null && !string.IsNullOrEmpty(experiment.Time) && experiment.Time != null) {
                if (!columnIndexes.ContainsKey(experiment.Time)) {
                    throw new Exception($"Failed to find time field {experiment.Time} for experiment {experiment.idExperiment}");
                }
                mapping.TimeField = columnIndexes[experiment.Time];
            }

            // Substance mapping(s)
            var substanceCodes = experiment.Substances
                .Split(',')
                .Select(r => r.Trim())
                .ToList();
            mapping.SubstanceMappings = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var substance in substanceCodes) {
                if (!columnIndexes.ContainsKey(substance)) {
                    throw new Exception($"Failed to find substance field {substance} for experiment {experiment.idExperiment}");
                }
                mapping.SubstanceMappings.Add(substance, columnIndexes[substance]);
            }

            // Response mapping(s)
            var responseFields = new Dictionary<string, ResponseMapping>(StringComparer.OrdinalIgnoreCase);
            mapping.ResponseMappings = responseFields;
            var responseCodes = experiment.Responses
                .Split(',')
                .Select(r => r.Trim())
                .ToList();
            foreach (var response in responseCodes) {
                var responseField = new ResponseMapping();
                if (!columnIndexes.ContainsKey(response)) {
                    throw new Exception($"Failed to find response field {response} for experiment {experiment.idExperiment}");
                }
                responseField.ColumnValue = columnKey(columnIndexes, response, ResponseValueType.Value);
                responseField.ColumnSd = columnKey(columnIndexes, response, ResponseValueType.SD);
                responseField.ColumnCv = columnKey(columnIndexes, response, ResponseValueType.CV);
                responseField.ColumnN = columnKey(columnIndexes, response, ResponseValueType.N);
                responseField.ColumnUncertainty = columnKey(columnIndexes, response, ResponseValueType.Uncertainty);
                responseFields.Add(response, responseField);
            }

            // Covariate mapping(s)
            mapping.CovariateMappings = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (experiment.Covariates != null && !string.IsNullOrEmpty(experiment.Covariates)) {
                var covariateCodes = experiment.Covariates
                    .Split(',')
                    .Select(r => r.Trim())
                    .ToList();
                foreach (var covariate in covariateCodes) {
                    if (!columnIndexes.ContainsKey(covariate)) {
                        throw new Exception($"Failed to find covariate field {covariate} for experiment {experiment.idExperiment}");
                    }
                    mapping.CovariateMappings.Add(covariate, columnIndexes[covariate]);
                }
            }

            responseMappings.Add(experiment.idExperiment, mapping);
            return responseMappings;
        }

        public static double? GetDoubleOrNull(IDataReader r, int? field) {
            if (field == null || r.IsDBNull((int)field)) {
                return null;
            }
            return Convert.ToSingle(r.GetValue((int)field));
        }

        public static float? GetFloatOrNull(IDataReader r, int? field) {
            if (field == null || r.IsDBNull((int)field)) {
                return (float?)null;
            }
            return Convert.ToSingle(r.GetValue((int)field));
        }

        public static float GetFloat(IDataReader r, int? field) {
            if (field == null || r.IsDBNull((int)field)) {
                throw new Exception($"Value for field {field} is missing (required)");
            }
            return Convert.ToSingle(r.GetValue((int)field));
        }

        private int? columnKey(Dictionary<string, int> columnIndexes, string response, ResponseValueType type) {
            if (type == ResponseValueType.Value) {
                if (columnIndexes.ContainsKey(response)) {
                    return columnIndexes[response];
                }
            }
            if (columnIndexes.ContainsKey(type.ToString() + ":" + response)) {
                return columnIndexes[type.ToString() + ":" + response];
            }
            return null;
        }

        #endregion

    }
}
