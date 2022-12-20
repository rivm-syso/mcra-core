using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class NonDietaryExposureSourcesBulkCopier : RawDataSourceBulkCopierBase {

        public NonDietaryExposureSourcesBulkCopier(
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
            progressState.Update("Processing non-dietary exposure sources");
            var hasNonDietaryExposureSources = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.NonDietaryExposureSources);
            if (hasNonDietaryExposureSources) {
                registerTableGroup(SourceTableGroup.NonDietaryExposureSources);
            }
            progressState.MarkCompleted();
        }
    }
}
