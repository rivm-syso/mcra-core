using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class SoilExposureDeterminantsBulkCopier : RawDataSourceBulkCopierBase {

        public SoilExposureDeterminantsBulkCopier(
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
            progressState.Update("Processing Soil exposure determinants");
            var hasSoilIngestions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.SoilIngestions);
            var hasSoilBioaccessibilityFractions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.SoilBioaccessibilityFractions);
            if (hasSoilIngestions) {
                registerTableGroup(SourceTableGroup.SoilExposureDeterminants);
            }
            progressState.Update(100);
        }
    }
}
