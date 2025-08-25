using MCRA.General;
using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class BurdensOfDiseaseBulkCopier : RawDataSourceBulkCopierBase {

        public BurdensOfDiseaseBulkCopier(
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
            progressState.Update("Processing burden of disease");
            var hasBurdensOfDisease = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.BurdensOfDisease);
            tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.BodIndicatorConversions);
            if (hasBurdensOfDisease) {
                registerTableGroup(SourceTableGroup.BurdensOfDisease);
            }
            progressState.Update(100);
        }
    }
}

