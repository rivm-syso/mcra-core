using MCRA.General;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class ExposureEffectFunctionsBulkCopier : RawDataSourceBulkCopierBase {

        public ExposureEffectFunctionsBulkCopier(
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
            progressState.Update("Processing exposure effect functions");
            var hasExposureEffectFunctions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ExposureEffectFunctions);
            if (hasExposureEffectFunctions) {
                registerTableGroup(SourceTableGroup.ExposureEffectFunctions);
            }
            progressState.Update(100);
        }
    }
}

