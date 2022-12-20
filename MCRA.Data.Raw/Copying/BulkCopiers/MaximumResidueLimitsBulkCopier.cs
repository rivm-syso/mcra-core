using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class MaximumResidueLimitsBulkCopier : RawDataSourceBulkCopierBase {

        public MaximumResidueLimitsBulkCopier(
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
            progressState.Update("Processing maximum residue limits");
            var hasMaximumConcentrationLimits = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.MaximumResidueLimits);
            if (hasMaximumConcentrationLimits) {
                registerTableGroup(SourceTableGroup.MaximumResidueLimits);
            }
            progressState.Update(100);
        }
    }
}
