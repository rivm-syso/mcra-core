using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class KineticConversionFactorsBulkCopier : RawDataSourceBulkCopierBase {

        public KineticConversionFactorsBulkCopier(
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
            var hasKineticConversionFactors = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.KineticConversionFactors);
            if (hasKineticConversionFactors) {
                registerTableGroup(SourceTableGroup.KineticConversionFactors);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.KineticConversionFactorSGs);
            }
        }
    }
}
