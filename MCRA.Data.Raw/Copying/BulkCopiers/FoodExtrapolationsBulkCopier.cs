using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class FoodExtrapolationsBulkCopier : RawDataSourceBulkCopierBase {

        public FoodExtrapolationsBulkCopier(
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
            progressState.Update("Processing food extrapolations");
            var hasFoodExtrapolationsData = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.FoodExtrapolations);
            if (hasFoodExtrapolationsData) {
                registerTableGroup(SourceTableGroup.FoodExtrapolations);
            }

            progressState.Update(100);
        }
    }
}
