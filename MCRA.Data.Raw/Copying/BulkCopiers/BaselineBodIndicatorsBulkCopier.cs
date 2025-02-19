using MCRA.General;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class BaselineBodIndicatorsBulkCopier : RawDataSourceBulkCopierBase {

        public BaselineBodIndicatorsBulkCopier(
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
            progressState.Update("Processing baseline burden of disease indicators");
            var hasBaselineBodIndicators = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.BaselineBodIndicators);
            if (hasBaselineBodIndicators) {
                registerTableGroup(SourceTableGroup.BaselineBodIndicators);
            }
            progressState.Update(100);
        }
    }
}

