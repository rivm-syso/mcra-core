using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class PbkModelsBulkCopier : RawDataSourceBulkCopierBase {

        public PbkModelsBulkCopier(
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
            var hasPbkModels = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.KineticModelInstances);
            if (hasPbkModels) {
                var hasPbkModelParameters = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.KineticModelInstanceParameters);
                if (hasPbkModelParameters) {
                    registerTableGroup(SourceTableGroup.PbkModels);
                } else {
                    throw new Exception("PBK model instances table found withouth PBK model parameters table.");
                }
            }
        }
    }
}
