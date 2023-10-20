using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class HazardCharacterisationsBulkCopier : RawDataSourceBulkCopierBase {

        public HazardCharacterisationsBulkCopier(
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
            progressState.Update("Processing hazard characterisations", 0);

            var hasHazardCharacterisations = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.HazardCharacterisations);
            if (hasHazardCharacterisations) {
                progressState.Update("Processing hazard characterisations uncertainty tables", 60);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.HazardCharacterisationsUncertain);
            }

            if (hasHazardCharacterisations) {
                registerTableGroup(SourceTableGroup.HazardCharacterisations);
            }

            progressState.Update(100);
        }
    }
}
