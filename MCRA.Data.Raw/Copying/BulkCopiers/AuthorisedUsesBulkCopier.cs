using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class AuthorisedUsesBulkCopier : RawDataSourceBulkCopierBase {

        public AuthorisedUsesBulkCopier(
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
            progressState.Update("Processing authorised uses");
            var hasAuthorisedUses = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AuthorisedUses);
            if (hasAuthorisedUses) {
                registerTableGroup(SourceTableGroup.AuthorisedUses);
            }
            progressState.Update(100);
        }
    }
}
