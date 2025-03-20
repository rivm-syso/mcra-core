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
            progressState.Update("Processing AirExposureDeterminants");
            var hasAirIndoorFractions = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AirIndoorFractions);
            var hasAirVentilatoryFlowRates = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AirVentilatoryFlowRates);
            if (hasAirIndoorFractions && hasAirVentilatoryFlowRates) {
                registerTableGroup(SourceTableGroup.AirExposureDeterminants);
            }
            progressState.Update(100);
        }
    }
}
