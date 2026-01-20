using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class OccupationalScenariosBulkCopier : RawDataSourceBulkCopierBase {

        public OccupationalScenariosBulkCopier(
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
            progressState.Update("Processing occupational scenarioss");
            var hasOccupationalTasks = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.OccupationalTasks);
            var hasOccupationalScenarios = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.OccupationalScenarios);
            var hasOccupationalScenarioTasks = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.OccupationalScenarioTasks);
            if (hasOccupationalTasks && hasOccupationalScenarios && hasOccupationalScenarioTasks) {
                registerTableGroup(SourceTableGroup.OccupationalScenarios);
            }
            progressState.Update(100);
        }
    }
}
