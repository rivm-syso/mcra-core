using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class InterSpeciesFactorsBulkCopier : RawDataSourceBulkCopierBase {

        public InterSpeciesFactorsBulkCopier(
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
            progressState.Update("Processing inter species model parameters", 0);
            var hasData = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.InterSpeciesModelParameters);
            if (hasData) {
                registerTableGroup(SourceTableGroup.InterSpeciesFactors);
            }
            progressState.Update(100);
        }
    }
}
