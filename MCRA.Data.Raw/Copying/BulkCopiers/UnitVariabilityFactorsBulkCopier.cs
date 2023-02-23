using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class UnitVariabilityFactorsBulkCopier : RawDataSourceBulkCopierBase {

        public UnitVariabilityFactorsBulkCopier(
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
            progressState.Update("Processing unit variability factors");
            var hasVariability = false;
            hasVariability |= tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.UnitVariabilityFactors);
            if (hasVariability) {
                progressState.Update("Post harvest applications", 90);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.IestiSpecialCases);
                registerTableGroup(SourceTableGroup.UnitVariabilityFactors);
            }
            progressState.Update(100);
        }
    }
}
