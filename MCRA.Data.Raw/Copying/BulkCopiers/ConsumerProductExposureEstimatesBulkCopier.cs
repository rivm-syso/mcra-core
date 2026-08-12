using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class ConsumerProductExposureEstimatesBulkCopier : RawDataSourceBulkCopierBase {

        public ConsumerProductExposureEstimatesBulkCopier(
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
            progressState.Update("Processing Consumer product exposure estimates");
            var hasConsumerProductsExposureAssessments = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ConsumerProductExposureAssessments);
            if (hasConsumerProductsExposureAssessments) {
                var hasConsumerProductsExposureSets = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ConsumerProductExposureSets);
                if (hasConsumerProductsExposureSets) {
                    var hasConsumerProductsExposurePercentiles = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ConsumerProductExposurePercentiles);
                    if (hasConsumerProductsExposurePercentiles) {
                        registerTableGroup(SourceTableGroup.ConsumerProductExposureEstimates);
                    }
                }
                progressState.Update(100);
            }
        }
    }
}
