using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class ConsumerProductExposureDeterminantsBulkCopier : RawDataSourceBulkCopierBase {

        public ConsumerProductExposureDeterminantsBulkCopier(
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
            progressState.Update("Processing consumer product exposure determinants");
            var hasExposureFractions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ConsumerProductExposureFractions);
            var hasApplicationAmounts = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ConsumerProductApplicationAmounts);
            if (hasExposureFractions && hasApplicationAmounts) {
                registerTableGroup(SourceTableGroup.ConsumerProductExposureDeterminants);
            }
            progressState.Update(100);
        }
    }
}
