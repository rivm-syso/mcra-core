using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class FoodSubstanceConversionFactorsBulkCopier : RawDataSourceBulkCopierBase {

        public FoodSubstanceConversionFactorsBulkCopier(
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
            progressState.Update("Food substance conversion factors", 10);
            var hasFoodSubstanceConversionFactors = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.DeterministicSubstanceConversionFactors);
            if (hasFoodSubstanceConversionFactors) {
                registerTableGroup(SourceTableGroup.FoodSubstanceConversionFactors);
            }
            progressState.Update(100);
        }
    }
}
