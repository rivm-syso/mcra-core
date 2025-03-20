using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class OutdoorAirConcentrationsBulkCopier : RawDataSourceBulkCopierBase {

        public OutdoorAirConcentrationsBulkCopier(
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
            progressState.Update("Processing OutdoorAirConcentrations");
            var hasOutdoorAirConcentrations = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.OutdoorAirConcentrations);
            if (hasOutdoorAirConcentrations) {
                registerTableGroup(SourceTableGroup.OutdoorAirConcentrations);
            }
            progressState.Update(100);
        }
    }
}
