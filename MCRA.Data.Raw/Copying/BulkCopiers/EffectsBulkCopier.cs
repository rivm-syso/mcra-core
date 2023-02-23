using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class EffectsBulkCopier : RawDataSourceBulkCopierBase {

        public EffectsBulkCopier(
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
            progressState.Update("Processing effects");
            var hasRawEffects = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.Effects);

            if (hasRawEffects) {
                registerTableGroup(SourceTableGroup.Effects);
            }

            progressState.Update(100);
        }
    }
}
