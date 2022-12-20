using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class IntraSpeciesFactorsBulkCopier : RawDataSourceBulkCopierBase {

        public IntraSpeciesFactorsBulkCopier(
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
            progressState.Update("Processing intra species model parameters", 0);
            var hasData = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.IntraSpeciesModelParameters);
            if (hasData) {
                registerTableGroup(SourceTableGroup.IntraSpeciesFactors);
            }
            progressState.Update(100);
        }
    }
}
