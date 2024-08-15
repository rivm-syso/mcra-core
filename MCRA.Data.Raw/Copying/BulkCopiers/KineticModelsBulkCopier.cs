using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class KineticModelsBulkCopier : RawDataSourceBulkCopierBase {

        public KineticModelsBulkCopier(
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
            var hasKineticAbsorptionFactors = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.KineticAbsorptionFactors);
            var hasKineticConversionFactors = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.KineticConversionFactors);
            if (hasKineticConversionFactors) {
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.KineticConversionFactorSGs);
            }
            if (hasKineticAbsorptionFactors || hasKineticConversionFactors) {
                registerTableGroup(SourceTableGroup.KineticModels);
            }
        }
    }
}
