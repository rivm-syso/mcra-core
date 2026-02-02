using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class SoilConcentrationsBulkCopier : RawDataSourceBulkCopierBase {

        public SoilConcentrationsBulkCopier(
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
            progressState.Update("Processing Soil concentrations");
            var hasSoilConcentrations = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.SoilConcentrations);
            if (hasSoilConcentrations) {
                registerTableGroup(SourceTableGroup.SoilConcentrations);
            }
            progressState.Update(100);
        }
    }
}
