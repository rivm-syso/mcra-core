using MCRA.General;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class AirConcentrationDistributionsBulkCopier : RawDataSourceBulkCopierBase {
        public AirConcentrationDistributionsBulkCopier(
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
            progressState.Update("Processing indoor, outdoor air concentration distributions");
            var hasIndoorConcentrationDistributions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.IndoorAirDistributions);
            var hasOutdoorConcentrationDistributions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.OutdoorAirDistributions);
            if (hasIndoorConcentrationDistributions || hasOutdoorConcentrationDistributions) {
                registerTableGroup(SourceTableGroup.AirConcentrationDistributions);
            }
            progressState.Update(100);
        }
    }
}
