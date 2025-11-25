using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Raw.Copying.BulkCopiers.HumanMonitoring;
using MCRA.General;
using System.Data;
using System.Globalization;
using MCRA.General.TableDefinitions.RawTableObjects;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class IndividualsBulkCopier : IndividualSetBulkCopier {

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

        public IndividualsBulkCopier(
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
            var hasIndividualSets = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.IndividualSets);
            if (hasIndividualSets) {
                progressState.Update("Processing Individuals", 10);
                if (!tryCopyIndividuals(dataSourceReader)) {
                    var msg = "Failed to copy individuals data. Missing individuals data.";
                    throw new RawDataSourceBulkCopyException(msg);
                }
                registerTableGroup(SourceTableGroup.Individuals);
            }

            //if (hasSurveys && hasSamples) {
            //    progressState.Update("Processing Individuals", 10);
            //    if (!tryCopyIndividuals(dataSourceReader)) {
            //        var msg = "Failed to copy human monitoring data. Missing individuals data.";
            //        throw new RawDataSourceBulkCopyException(msg);
            //    }
            //    tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.HumanMonitoringTimepoints);

            //    if (!_parsedDataTables.Contains(RawDataSourceTableID.AnalyticalMethods)) {
            //        progressState.Update("Processing analytical methods", 40);
            //        var hasAnalyticalMethods = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AnalyticalMethods);
            //        var hasAnalyticalMethodCompounds = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AnalyticalMethodCompounds);
            //    }

            //    progressState.Update("Processing sample analyses", 60);
            //    var analyticalMethods = readAnalyticalMethods(dataSourceReader);
            //    var humanMonitoringSampleAnalyses = readSampleAnalyses(dataSourceReader, analyticalMethods);
            //    var humanMonitoringSampleAnalysesDict = humanMonitoringSampleAnalyses.ToDictionary(r => r.idSampleAnalysis);
            //    var humanMonitoringSampleAnalysesTable = humanMonitoringSampleAnalyses.ToDataTable();
            //    humanMonitoringSampleAnalysesTable.Columns.Remove("Concentrations");

            //    if (!tryCopyDataTable(humanMonitoringSampleAnalysesTable, RawDataSourceTableID.HumanMonitoringSampleAnalyses)) {
            //        var msg = $"Failed to copy human monitoring sample analyses data.";
            //        throw new RawDataSourceBulkCopyException(msg);
            //    }

            //    progressState.Update("Processing sample concentrations", 80);
            //    var hasSampleConcentrations = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.HumanMonitoringSampleConcentrations);
            //    if (!hasSampleConcentrations) {
            //        var humanMonitoringSampleConcentrations = readSampleConcentrations(dataSourceReader, analyticalMethods, humanMonitoringSampleAnalysesDict);
            //        var humanMonitoringSampleConcentrationsTable = humanMonitoringSampleConcentrations.ToDataTable();
            //        if (!tryCopyDataTable(humanMonitoringSampleConcentrationsTable, RawDataSourceTableID.HumanMonitoringSampleConcentrations)) {
            //            var msg = $"Failed to copy human monitoring sample concentrations.";
            //            throw new RawDataSourceBulkCopyException(msg);
            //        }
            //    }

            //    registerTableGroup(SourceTableGroup.HumanMonitoringData);
            //} else if (hasSurveys) {
            //    var msg = $"Found human monitoring survey definition, but could not find samples table.";
            //    throw new RawDataSourceBulkCopyException(msg);
            //} else if (hasSamples) {
            //    var msg = $"Found human monitoring samples, but could not find human monitoring surveys table.";
            //    throw new RawDataSourceBulkCopyException(msg);
            //}

            progressState.Update(100);
        }
       
    }
}
