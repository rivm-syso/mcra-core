using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class IndoorAirConcentrationsBulkCopier : RawDataSourceBulkCopierBase {

        public IndoorAirConcentrationsBulkCopier(
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
            progressState.Update("Processing IndoorAirConcentrations");
            var hasIndoorAirConcentrations = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.IndoorAirConcentrations);
            if (hasIndoorAirConcentrations) {
                registerTableGroup(SourceTableGroup.IndoorAirConcentrations);
            }
            progressState.Update(100);
        }
    }
}
