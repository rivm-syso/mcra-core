using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class ResponsesBulkCopier : RawDataSourceBulkCopierBase {

        public ResponsesBulkCopier(
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
            var hasResponses = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.Responses);
            if (hasResponses) {
                registerTableGroup(SourceTableGroup.Responses);
            }
        }
    }
}
