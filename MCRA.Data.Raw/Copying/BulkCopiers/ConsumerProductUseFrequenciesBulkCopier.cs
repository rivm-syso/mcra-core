using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Data.Raw.Copying.BulkCopiers.HumanMonitoring;
using MCRA.General;
using System.Data;
using System.Globalization;
using MCRA.General.TableDefinitions.RawTableObjects;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class ConsumerProductUseFrequenciesBulkCopier : IndividualSetBulkCopier {

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

        public ConsumerProductUseFrequenciesBulkCopier(
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
            var hasSurveys = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ConsumerProductSurveys);
            var hasFrequencies = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ConsumerProductUseFrequencies);

            if (hasSurveys && hasFrequencies) {
                progressState.Update("Processing Individuals", 10);
                if (!tryCopyIndividuals(dataSourceReader)) {
                    var msg = "Failed to copy consumer product use frequencies. Missing individuals data.";
                    throw new RawDataSourceBulkCopyException(msg);
                }

                registerTableGroup(SourceTableGroup.ConsumerProductUseFrequencies);
            } 
            progressState.Update(100);
        }
    }
}
