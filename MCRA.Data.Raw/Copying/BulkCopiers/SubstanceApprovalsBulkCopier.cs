using MCRA.General;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class SubstanceApprovalsBulkCopier : RawDataSourceBulkCopierBase {

        public SubstanceApprovalsBulkCopier(
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
            progressState.Update("Processing substance approvals");
            var hasSubstanceApprovals = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.SubstanceApprovals);
            if (hasSubstanceApprovals) {
                registerTableGroup(SourceTableGroup.SubstanceApprovals);
            }
            progressState.Update(100);
        }
    }
}
