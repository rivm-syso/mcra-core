using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class OccupationalTaskExposuresBulkCopier : RawDataSourceBulkCopierBase {

        public OccupationalTaskExposuresBulkCopier(
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
            progressState.Update("Processing occupational task exposures");
            var hasOccupationalTaskExposures = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.OccupationalTaskExposures);
            if (hasOccupationalTaskExposures) {
                registerTableGroup(SourceTableGroup.OccupationalTaskExposures);
            }
            progressState.Update(100);
        }
    }
}
