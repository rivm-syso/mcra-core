using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class HazardDosesBulkCopier : RawDataSourceBulkCopierBase {

        public HazardDosesBulkCopier(
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
            progressState.Update("Processing points of departure doses", 0);

            var hasPointsOfDeparture = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.HazardDoses);
            if (hasPointsOfDeparture) {
                progressState.Update("Processing points of departure uncertainty tables", 60);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.HazardDosesUncertain);
            }

            if (hasPointsOfDeparture) {
                registerTableGroup(SourceTableGroup.HazardDoses);
            }

            progressState.Update(100);
        }
    }
}
