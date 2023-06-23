using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class AgriculturalUseBulkCopier : RawDataSourceBulkCopierBase {

        public AgriculturalUseBulkCopier(
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
            progressState.Update("Processing agricultural uses");
            var hasAgriculturalUses = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AgriculturalUses);
            if (hasAgriculturalUses) {
                var hasSubstanceLinks = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AgriculturalUsesHasCompounds);
                if (hasSubstanceLinks) {
                    registerTableGroup(SourceTableGroup.AgriculturalUse);
                } else {
                    const string message = "Error in copying the agricultural uses: failed to find the substance links for the specified agricultural uses.";
                    throw new RawDataSourceBulkCopyException(message);
                }
            }
            progressState.Update(100);
        }
    }
}
