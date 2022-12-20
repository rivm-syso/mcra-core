using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class FoodTranslationsBulkCopier : RawDataSourceBulkCopierBase {

        public FoodTranslationsBulkCopier(
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
            progressState.Update("Processing FoodTranslations");
            var hasFoodTranslations = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.FoodTranslations);
            if (hasFoodTranslations) {
                registerTableGroup(SourceTableGroup.FoodTranslations);
            }
            progressState.Update(100);
        }
    }
}
