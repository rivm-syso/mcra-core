using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class BackgroundFoodConcentrationsBulkCopier : RawDataSourceBulkCopierBase {

        public BackgroundFoodConcentrationsBulkCopier(
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
            progressState.Update("Background food concentrations", 10);
            var hasBackgroundFoodConcentrations = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.BackgroundFoodConcentrations);
            if (hasBackgroundFoodConcentrations) {
                registerTableGroup(SourceTableGroup.BackgroundFoodConcentrations);
            }
            progressState.Update(100);
        }
    }
}
