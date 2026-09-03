using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class FocalProcessingBulkCopier : RawDataSourceBulkCopierBase {

        public FocalProcessingBulkCopier(
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
            progressState.Update("Focal processing factors", 10);
            var hasFocalProcessingFactors = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ProcessingFactors);
            if (hasFocalProcessingFactors) {
                registerTableGroup(SourceTableGroup.FocalProcessing);
            }
            progressState.Update(100);
        }
    }
}
