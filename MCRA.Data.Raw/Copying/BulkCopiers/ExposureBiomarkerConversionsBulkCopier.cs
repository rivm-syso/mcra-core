using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class ExposureBiomarkerConversionsBulkCopier : RawDataSourceBulkCopierBase {

        public ExposureBiomarkerConversionsBulkCopier(
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
            progressState.Update("Processing Exposure biomarker conversions");
            var hasExposureBiomarkerConversions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ExposureBiomarkerConversions);
            if (hasExposureBiomarkerConversions) {
                registerTableGroup(SourceTableGroup.ExposureBiomarkerConversions);
            }
            progressState.Update(100);
        }
    }
}
