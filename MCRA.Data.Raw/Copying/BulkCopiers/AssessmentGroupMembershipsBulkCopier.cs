using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class AssessmentGroupMembershipsBulkCopier : RawDataSourceBulkCopierBase {

        public AssessmentGroupMembershipsBulkCopier(
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
            progressState.Update($"Processing { SourceTableGroup.AssessmentGroupMemberships.GetDisplayName() }");
            var hasModels = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AssessmentGroupMembershipModels);
            if (hasModels) {
                var hasData = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AssessmentGroupMemberships);
                if (hasData) {
                    registerTableGroup(SourceTableGroup.AssessmentGroupMemberships);
                }
            }
            progressState.Update(100);
        }
    }
}
