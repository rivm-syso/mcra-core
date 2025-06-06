using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class ConsumerProductConcentrationsBulkCopier : RawDataSourceBulkCopierBase {

        public ConsumerProductConcentrationsBulkCopier(
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
            progressState.Update("Processing Consumer product concentrations");
            var hasConsumerProductsConcentrations = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ConsumerProductConcentrations);
            if (hasConsumerProductsConcentrations) {
                registerTableGroup(SourceTableGroup.ConsumerProductConcentrations);
            }
            progressState.Update(100);
        }
    }
}
