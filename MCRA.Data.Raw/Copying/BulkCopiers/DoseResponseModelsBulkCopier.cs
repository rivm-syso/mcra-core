using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class DoseResponseModelsBulkCopier : RawDataSourceBulkCopierBase {

        public DoseResponseModelsBulkCopier(
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
            progressState.Update("Processing dose response models");
            var hasRawDRModels = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.DoseResponseModels);
            if (hasRawDRModels) {
                progressState.Update("Processing dose response model benchmark doses", 50);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.DoseResponseModelBenchmarkDoses);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.DoseResponseModelBenchmarkDosesUncertain);
                registerTableGroup(SourceTableGroup.DoseResponseModels);
            }
            progressState.Update(100);
        }
    }
}
