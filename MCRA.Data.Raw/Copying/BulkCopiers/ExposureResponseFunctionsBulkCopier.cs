using MCRA.General;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class ExposureResponseFunctionsBulkCopier : RawDataSourceBulkCopierBase {

        public ExposureResponseFunctionsBulkCopier(
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
            progressState.Update("Processing exposure response functions", 60);
            var hasExposureResponseFunctions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ExposureResponseFunctions);
            if (hasExposureResponseFunctions) {
                registerTableGroup(SourceTableGroup.ExposureResponseFunctions);
            }

            if (hasExposureResponseFunctions) {
                var hasErfSubgroups = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ErfSubgroups);
                if (hasErfSubgroups) {
                    progressState.Update("Processing exposure response functions subgroups tables", 60);
                }
            }

            progressState.Update(100);
        }
    }
}

