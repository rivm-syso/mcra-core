using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class TotalDietStudyBulkCopier : RawDataSourceBulkCopierBase {

        public TotalDietStudyBulkCopier(
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
            progressState.Update("Processing TDS food/sample compositions");
            var hasTotalDietStudyData = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.TdsFoodSampleCompositions);
            if (hasTotalDietStudyData) {
                registerTableGroup(SourceTableGroup.TotalDietStudy);
            }
            progressState.Update(100);
        }
    }
}
