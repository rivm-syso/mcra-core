using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class TestSystemsBulkCopier : RawDataSourceBulkCopierBase {

        public TestSystemsBulkCopier(
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
            var hasSystems = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.TestSystems);
            if (hasSystems) {
                registerTableGroup(SourceTableGroup.TestSystems);
            }
        }
    }
}
