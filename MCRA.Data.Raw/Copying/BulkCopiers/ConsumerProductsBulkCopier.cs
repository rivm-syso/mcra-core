using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class ConsumerProductsBulkCopier : RawDataSourceBulkCopierBase {

        public ConsumerProductsBulkCopier(
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
            progressState.Update("Processing Consumer products");
            var hasConsumerProducts = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ConsumerProducts);
            if (hasConsumerProducts) {
                registerTableGroup(SourceTableGroup.ConsumerProducts);
            }

            progressState.MarkCompleted();
        }
    }
}
