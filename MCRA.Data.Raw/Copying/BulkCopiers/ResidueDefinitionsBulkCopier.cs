using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class ResidueDefinitionsBulkCopier : RawDataSourceBulkCopierBase {

        public ResidueDefinitionsBulkCopier(
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
            progressState.Update("Processing substance translations");
            if (tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ResidueDefinitions)) {
                registerTableGroup(SourceTableGroup.ResidueDefinitions);
            }
            progressState.Update(100);
        }
    }
}
