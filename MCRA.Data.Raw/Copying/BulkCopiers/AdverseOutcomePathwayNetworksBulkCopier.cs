using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class AdverseOutcomePathwayNetworksBulkCopier : RawDataSourceBulkCopierBase {

        public AdverseOutcomePathwayNetworksBulkCopier(
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
            progressState.Update("AOP networks");
            var hasAdverseOutcomePathwayNetworks = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.AdverseOutcomePathwayNetworks);
            if (hasAdverseOutcomePathwayNetworks) {
                var hasEffectRelations = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.EffectRelations);
                if (!hasEffectRelations) {
                    throw new RawDataSourceBulkCopyException("Cannot find effect relations table that should accompany the table AOP networks");
                }
                registerTableGroup(SourceTableGroup.AdverseOutcomePathwayNetworks);
            }
            progressState.Update(100);
        }
    }
}
