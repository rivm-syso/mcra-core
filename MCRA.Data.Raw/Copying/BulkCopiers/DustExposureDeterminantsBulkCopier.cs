using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class DustExposureDeterminantsBulkCopier : RawDataSourceBulkCopierBase {

        public DustExposureDeterminantsBulkCopier(
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
            progressState.Update("Processing Dust exposure determinants");
            var hasDustIngestions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.DustIngestions);
            var hasDustBodyExposureFractions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.DustBodyExposureFractions);
            var hasDustAdherenceAmount = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.DustAdherenceAmounts);
            var hasDustAvailabilityFractions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.DustAvailabilityFractions);
            var hasDustBioaccessibilityFractions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.DustBioaccessibilityFractions);
            if (hasDustIngestions
                && hasDustBodyExposureFractions
                && hasDustAdherenceAmount
                && hasDustAvailabilityFractions
            ) {
                registerTableGroup(SourceTableGroup.DustExposureDeterminants);
            }
            progressState.Update(100);
        }
    }
}
