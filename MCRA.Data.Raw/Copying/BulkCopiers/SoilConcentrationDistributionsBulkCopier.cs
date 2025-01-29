using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class SoilConcentrationDistributionsBulkCopier : RawDataSourceBulkCopierBase {

        public SoilConcentrationDistributionsBulkCopier(
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
            progressState.Update("Processing SoilConcentrationDistributions");
            var hasSoilConcentrationDistributions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.SoilConcentrationDistributions);
            if (hasSoilConcentrationDistributions) {
                registerTableGroup(SourceTableGroup.SoilConcentrationDistributions);
            }
            progressState.Update(100);
        }
    }
}
