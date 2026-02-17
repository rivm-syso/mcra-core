using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class AirExposureDeterminantsBulkCopier : RawDataSourceBulkCopierBase {

        public AirExposureDeterminantsBulkCopier(
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
            progressState.Update("Processing Air Exposure Determinants");
            var hasIndoorFractions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AirIndoorFractions);
            var hasVentilatoryFlowRates = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AirVentilatoryFlowRates);
            var hasBodyExposureFractions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AirBodyExposureFractions);
            if ((hasIndoorFractions && hasVentilatoryFlowRates) || hasBodyExposureFractions) {
                registerTableGroup(SourceTableGroup.AirExposureDeterminants);
            }
            progressState.Update(100);
        }
    }
}
