using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class ConsumerProductConcentrationDistributionsBulkCopier : RawDataSourceBulkCopierBase {

        public ConsumerProductConcentrationDistributionsBulkCopier(
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
            progressState.Update("Processing Consumer product concentration distributions");
            var hasConsumerProductConcentrationDistributions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ConsumerProductConcentrationDistributions);
            if (hasConsumerProductConcentrationDistributions) {
                registerTableGroup(SourceTableGroup.ConsumerProductConcentrationDistributions);
            }
            progressState.Update(100);
        }
    }
}
