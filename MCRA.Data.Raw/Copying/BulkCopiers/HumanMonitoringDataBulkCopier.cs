using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Raw.Copying.BulkCopiers.HumanMonitoring;
using MCRA.General;
using System.Data;
using System.Globalization;
using MCRA.General.TableDefinitions.RawTableObjects;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class HumanMonitoringDataBulkCopier : IndividualSetBulkCopier {

        #region Helper classes

        internal class RawHumanMonitoringSampleAnalysisRecord {
            public string idSampleAnalysis { get; set; }
            public string idSample { get; set; }
            public string idAnalyticalMethod { get; set; }
            public DateTime? DateAnalysis { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public List<RawHumanMonitoringSampleConcentration> Concentrations { get; set; } = [];
        }

        #endregion

        public HumanMonitoringDataBulkCopier(
            IDataSourceWriter dataSourceWriter,
            HashSet<SourceTableGroup> parsedTableGroups,
            HashSet<RawDataSourceTableID> parsedDataTables)
            : base(
                  dataSourceWriter,
                  parsedTableGroups,
                  parsedDataTables
        ) {
        }

        public override void TryCopy(IDataSourceReader dataSourceReader, ProgressState progressState) {
            var hasSurveys = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.HumanMonitoringSurveys);
            var hasSamples = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.HumanMonitoringSamples);

            if (hasSurveys && hasSamples) {
                progressState.Update("Processing Individuals", 10);
                if (!tryCopyIndividuals(dataSourceReader)) {
                    var msg = "Failed to copy human monitoring data. Missing individuals data.";
                    throw new RawDataSourceBulkCopyException(msg);
                }
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.HumanMonitoringTimepoints);

                if (!_parsedDataTables.Contains(RawDataSourceTableID.AnalyticalMethods)) {
                    progressState.Update("Processing analytical methods", 40);
                    var hasAnalyticalMethods = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AnalyticalMethods);
                    var hasAnalyticalMethodCompounds = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AnalyticalMethodCompounds);
                }

                progressState.Update("Processing sample analyses", 60);
                var analyticalMethods = readAnalyticalMethods(dataSourceReader);
                var humanMonitoringSampleAnalyses = readSampleAnalyses(dataSourceReader, analyticalMethods);
                var humanMonitoringSampleAnalysesDict = humanMonitoringSampleAnalyses.ToDictionary(r => r.idSampleAnalysis);
                var humanMonitoringSampleAnalysesTable = humanMonitoringSampleAnalyses.ToDataTable();
                humanMonitoringSampleAnalysesTable.Columns.Remove("Concentrations");

                if (!tryCopyDataTable(humanMonitoringSampleAnalysesTable, RawDataSourceTableID.HumanMonitoringSampleAnalyses)) {
                    var msg = $"Failed to copy human monitoring sample analyses data.";
                    throw new RawDataSourceBulkCopyException(msg);
                }

                progressState.Update("Processing sample concentrations", 80);
                var hasSampleConcentrations = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.HumanMonitoringSampleConcentrations);
                if (!hasSampleConcentrations) {
                    var humanMonitoringSampleConcentrations = readSampleConcentrations(dataSourceReader, analyticalMethods, humanMonitoringSampleAnalysesDict);
                    var humanMonitoringSampleConcentrationsTable = humanMonitoringSampleConcentrations.ToDataTable();
                    if (!tryCopyDataTable(humanMonitoringSampleConcentrationsTable, RawDataSourceTableID.HumanMonitoringSampleConcentrations)) {
                        var msg = $"Failed to copy human monitoring sample concentrations.";
                        throw new RawDataSourceBulkCopyException(msg);
                    }
                }

                registerTableGroup(SourceTableGroup.HumanMonitoringData);
            } else if (hasSurveys) {
                var msg = $"Found human monitoring survey definition, but could not find samples table.";
                throw new RawDataSourceBulkCopyException(msg);
            } else if (hasSamples) {
                var msg = $"Found human monitoring samples, but could not find human monitoring surveys table.";
                throw new RawDataSourceBulkCopyException(msg);
            }

            progressState.Update(100);
        }

        private static List<RawHumanMonitoringSampleAnalysisRecord> readSampleAnalyses(IDataSourceReader dataSourceReader, Dictionary<string, RawAnalyticalMethodRecord> analyticalMethods) {
            var tableDefinitionSampleAnalyses = _tableDefinitions[RawDataSourceTableID.HumanMonitoringSampleAnalyses];
            var sampleAnalyses = dataSourceReader.ReadDataTable<RawHumanMonitoringSampleAnalysisRecord>(tableDefinitionSampleAnalyses);
            return sampleAnalyses;
        }

        private static List<RawHumanMonitoringSampleConcentration> readSampleConcentrations(
            IDataSourceReader dataSourceReader,
            Dictionary<string, RawAnalyticalMethodRecord> analyticalMethods,
            Dictionary<string, RawHumanMonitoringSampleAnalysisRecord> humanMonitoringSampleAnalyses
        ) {
            var result = new List<RawHumanMonitoringSampleConcentration>();

            var tableDefinitionSampleAnalyses = _tableDefinitions[RawDataSourceTableID.HumanMonitoringSampleAnalyses];
            using var sourceTableReader = dataSourceReader.GetDataReaderByDefinition(tableDefinitionSampleAnalyses, out string sourceTableName);
            if (sourceTableReader != null) {

                // Get column names
                var columnNames = sourceTableReader.GetColumnNames();

                // Get Column mappings
                var columnMappings = columnNames
                    .Select((r, ix) => new {
                        Index = ix,
                        ColumnDefinition = tableDefinitionSampleAnalyses.FindColumnDefinitionByAlias(r),
                        Name = r
                    })
                    .ToList();

                // Get field indexes of sample analysis id and analytical method id
                var idSampleAnalysisColumnDefinition = tableDefinitionSampleAnalyses.FindColumnDefinitionByAlias("idSampleAnalysis");
                var idAnalyticalMethodColumnDefinition = tableDefinitionSampleAnalyses.FindColumnDefinitionByAlias("idAnalyticalMethod");
                var idSampleAnalysisFieldIndex = columnMappings.First(r => r.ColumnDefinition == idSampleAnalysisColumnDefinition).Index;
                var idAnalyticalMethodFieldIndex = columnMappings.First(r => r.ColumnDefinition == idAnalyticalMethodColumnDefinition).Index;

                // Get unmapped columns
                var unMappedColumns = columnMappings
                    .Where(r => r.ColumnDefinition == null || r.ColumnDefinition.IsDynamic)
                    .ToDictionary(r => r.Name, StringComparer.InvariantCultureIgnoreCase);

                while (sourceTableReader.Read()) {
                    var idSampleAnalysis = Convert.ToString(sourceTableReader.GetValue(idSampleAnalysisFieldIndex));
                    var idAnalyticalMethod = Convert.ToString(sourceTableReader.GetValue(idAnalyticalMethodFieldIndex));
                    var analyticalMethod = analyticalMethods[idAnalyticalMethod];
                    var sampleAnalysis = humanMonitoringSampleAnalyses[idSampleAnalysis];
                    foreach (var analyticalMethodCompound in analyticalMethod.AnalyticalMethodCompounds) {
                        if (!unMappedColumns.ContainsKey(analyticalMethodCompound.Key)) {
                            var msg = $"Cannot find measurement values for substance {analyticalMethodCompound.Key} in human monitoring sample analyses table.";
                            throw new RawDataSourceBulkCopyException(msg);
                        }
                        var substanceConcentrationField = unMappedColumns[analyticalMethodCompound.Key].Index;

                        var valueString = Convert.ToString(sourceTableReader.GetValue(substanceConcentrationField), CultureInfo.InvariantCulture);
                        if (string.IsNullOrEmpty(valueString)) {
                            // Censored value
                            var concentrationRecord = new RawHumanMonitoringSampleConcentration() {
                                idAnalysisSample = idSampleAnalysis,
                                idCompound = analyticalMethodCompound.Key,
                                ResType = (analyticalMethodCompound.Value.LOQ.HasValue && !double.IsNaN(analyticalMethodCompound.Value.LOQ.Value))
                                    ? ResType.LOQ : ResType.LOD,
                            };
                            result.Add(concentrationRecord);
                        } else if (valueString.Equals(ResType.MV.ToString(), StringComparison.OrdinalIgnoreCase)) {
                            // Missing value
                            var concentrationRecord = new RawHumanMonitoringSampleConcentration() {
                                idAnalysisSample = idSampleAnalysis,
                                idCompound = analyticalMethodCompound.Key,
                                ResType = ResType.MV
                            };
                            result.Add(concentrationRecord);
                        } else {
                            // Positive value
                            if (double.TryParse(valueString, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out double concentration)) {
                                var concentrationRecord = new RawHumanMonitoringSampleConcentration() {
                                    idAnalysisSample = idSampleAnalysis,
                                    idCompound = analyticalMethodCompound.Key,
                                    Concentration = concentration,
                                    ResType = ResType.VAL
                                };
                                result.Add(concentrationRecord);
                            } else {
                                throw new Exception($"Unrecognized value {valueString} in human monitoring sample concentrations");
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static Dictionary<string, RawAnalyticalMethodRecord> readAnalyticalMethods(IDataSourceReader dataSourceReader) {

            // Read analytical methods
            var tableDefinitionAnalyticalMethods = _tableDefinitions[RawDataSourceTableID.AnalyticalMethods];
            var analyticalMethods = dataSourceReader.ReadDataTable<RawAnalyticalMethodRecord>(tableDefinitionAnalyticalMethods).ToList();
            var duplicateAnalyticalMethodKeys = analyticalMethods.GroupBy(r => r.idAnalyticalMethod).Where(g => g.Count() > 1);
            if (duplicateAnalyticalMethodKeys.Any()) {
                var msg = $"Duplicate key {duplicateAnalyticalMethodKeys.First().Key} in analytical methods table.";
                throw new RawDataSourceBulkCopyException(msg);
            }
            var analyticalMethodsDictionary = analyticalMethods.ToDictionary(r => r.idAnalyticalMethod);

            // Read analytical method compounds
            var tableDefinitionAnalyticalMethodCompounds = _tableDefinitions[RawDataSourceTableID.AnalyticalMethodCompounds];
            var analyticalMethodCompounds = dataSourceReader.ReadDataTable<RawAnalyticalMethodCompound>(tableDefinitionAnalyticalMethodCompounds).ToList();
            foreach (var record in analyticalMethodCompounds) {
                if (!analyticalMethodsDictionary.ContainsKey(record.idAnalyticalMethod)) {
                    var msg = $"Unknown reference to analytical method {record.idAnalyticalMethod} in analytical method compounds table.";
                    throw new RawDataSourceBulkCopyException(msg);
                }
                var analyticalMethod = analyticalMethodsDictionary[record.idAnalyticalMethod];
                if (analyticalMethod.AnalyticalMethodCompounds.ContainsKey(record.idCompound)) {
                    var msg = $"Duplicate substance definition for analytical method {record.idAnalyticalMethod} in analytical method compounds table.";
                    throw new RawDataSourceBulkCopyException(msg);
                }
                analyticalMethod.AnalyticalMethodCompounds.Add(record.idCompound, record);
            }

            return analyticalMethodsDictionary;
        }
    }
}
