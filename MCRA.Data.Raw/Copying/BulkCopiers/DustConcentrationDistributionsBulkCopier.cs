using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class DustConcentrationDistributionsBulkCopier : RawDataSourceBulkCopierBase {

        public DustConcentrationDistributionsBulkCopier(
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
            progressState.Update("Processing DustConcentrationDistributions");
            var hasDustConcentrationDistributions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.DustConcentrationDistributions);
            if (hasDustConcentrationDistributions) {
                registerTableGroup(SourceTableGroup.DustConcentrationDistributions);
            }
            progressState.Update(100);
        }
    }
}
