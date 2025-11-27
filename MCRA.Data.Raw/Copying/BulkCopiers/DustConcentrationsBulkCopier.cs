using MCRA.General;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class DustConcentrationsBulkCopier : RawDataSourceBulkCopierBase {

        public DustConcentrationsBulkCopier(
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
            progressState.Update("Processing DustConcentrations");
            var hasDustConcentrations = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.DustConcentrations);
            if (hasDustConcentrations) {
                registerTableGroup(SourceTableGroup.DustConcentrations);
            }
            progressState.Update(100);
        }
    }
}
