using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class SingleValueConsumptionsBulkCopier : RawDataSourceBulkCopierBase {

        public SingleValueConsumptionsBulkCopier(
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
            progressState.Update("Processing Single Value Consumption Data");
            var hasPopulationSingleValues = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.PopulationConsumptionSingleValues);
            if (hasPopulationSingleValues) {
                registerTableGroup(SourceTableGroup.SingleValueConsumptions);
            }

            progressState.Update(100);
        }
    }
}
