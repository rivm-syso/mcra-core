using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class SingleValueConcentrationsBulkCopier : RawDataSourceBulkCopierBase {

        public SingleValueConcentrationsBulkCopier(
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
            progressState.Update("Processing Single Value Concentration Data");
            var hasConcentrationSingleValues = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ConcentrationSingleValues);
            if (hasConcentrationSingleValues) {
                registerTableGroup(SourceTableGroup.SingleValueConcentrations);
            }

            progressState.Update(100);
        }
    }
}
