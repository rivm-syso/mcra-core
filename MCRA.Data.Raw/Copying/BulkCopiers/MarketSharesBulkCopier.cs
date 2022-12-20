using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class MarketSharesBulkCopier : RawDataSourceBulkCopierBase {

        public MarketSharesBulkCopier(
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
            progressState.Update("Processing MarketShares");
            var hasMarketShares = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.MarketShares);
            if (hasMarketShares) {
                registerTableGroup(SourceTableGroup.MarketShares);
            }
            progressState.Update(100);
        }
    }
}
