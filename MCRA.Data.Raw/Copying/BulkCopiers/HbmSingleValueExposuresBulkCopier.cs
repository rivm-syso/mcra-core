using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class HbmSingleValueExposuresBulkCopier : RawDataSourceBulkCopierBase {

        public HbmSingleValueExposuresBulkCopier(
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
            progressState.Update("Processing HBM single value exposures");
            var hasSVHbmExposureSurveys = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.HbmSingleValueExposureSurveys);
            var hasSVHbmExposureSets = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.HbmSingleValueExposureSets);
            if (hasSVHbmExposureSurveys && hasSVHbmExposureSets) {
                var hasSVHbmExposuresPercentiles = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.HbmSingleValueExposures);
                if (!hasSVHbmExposuresPercentiles) {
                    throw new RawDataSourceBulkCopyException("Failed to copy HBM single value exposures percentiles data.");
                }
                registerTableGroup(SourceTableGroup.HbmSingleValueExposures);
            }
            progressState.Update(100);
        }
    }
}
