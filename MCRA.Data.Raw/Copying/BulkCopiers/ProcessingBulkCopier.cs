using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class ProcessingBulkCopier : RawDataSourceBulkCopierBase {

        public ProcessingBulkCopier(
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

            progressState.Update("Processing factors", 10);
            var hasProcessingFactors = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ProcessingFactors);
            if (hasProcessingFactors) {
                registerTableGroup(SourceTableGroup.Processing);
            }
            progressState.Update(100);
        }
    }
}
