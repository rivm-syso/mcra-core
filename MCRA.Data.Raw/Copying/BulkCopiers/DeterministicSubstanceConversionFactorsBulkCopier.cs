using MCRA.Utils.DataFileReading;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class DeterministicSubstanceConversionFactorsBulkCopier : RawDataSourceBulkCopierBase {

        public DeterministicSubstanceConversionFactorsBulkCopier(
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
            progressState.Update($"Processing {SourceTableGroup.DeterministicSubstanceConversionFactors.GetDisplayName()}");
            var hasDeterministicSubstanceConversionFactors = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.DeterministicSubstanceConversionFactors);
            if (hasDeterministicSubstanceConversionFactors) {
                registerTableGroup(SourceTableGroup.DeterministicSubstanceConversionFactors);
            }
            progressState.Update(100);
        }
    }
}
