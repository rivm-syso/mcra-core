using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class ConcentrationDistributionsBulkCopier : RawDataSourceBulkCopierBase {

        public ConcentrationDistributionsBulkCopier(
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
            progressState.Update("Processing ConcentrationDistributions");
            var hasConcentrationDistributions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ConcentrationDistributions);
            if (hasConcentrationDistributions) {
                registerTableGroup(SourceTableGroup.ConcentrationDistributions);
            }
            progressState.Update(100);
        }
    }
}
