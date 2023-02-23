using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class EffectRepresentationsBulkCopier : RawDataSourceBulkCopierBase {

        public EffectRepresentationsBulkCopier(
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
            progressState.Update("Effect representations");
            var hasEffectRepresentations = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.EffectRepresentations);
            if (hasEffectRepresentations) {
                registerTableGroup(SourceTableGroup.EffectRepresentations);
            }
            progressState.Update(100);
        }
    }
}
