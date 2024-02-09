using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Data;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class SingleValueNonDietaryExposuresBulkCopier : RawDataSourceBulkCopierBase {

        public SingleValueNonDietaryExposuresBulkCopier(
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
            if (!_parsedDataTables.Contains(RawDataSourceTableID.ExposureScenarios)) {
                progressState.Update("Processing single value non-dietary exposure data");
                var hasScenarios = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ExposureScenarios);
                if (hasScenarios) {
                    
                    if (!tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ExposureDeterminants)) {
                        var msg = "Failed to copy exposure determinants, part of the single value non dietary exposures data.";
                        throw new RawDataSourceBulkCopyException(msg);
                    }
                    if (!tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ExposureDeterminantCombinations)) {
                        var msg = "Failed to copy exposure determinant combinations, part of the single value non dietary exposures data.";
                        throw new RawDataSourceBulkCopyException(msg);
                    }
                    if (!tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.ExposureEstimates)) {
                        var msg = "Failed to copy exposure estimates, part of the single value non dietary exposures data.";
                        throw new RawDataSourceBulkCopyException(msg);
                    }

                    tryDoBulkCopyWithDynamicPropertyValues(
                            dataSourceReader,
                            RawDataSourceTableID.ExposureDeterminantCombinations,
                            RawDataSourceTableID.ExposureDeterminantValues
                        );
                    registerTableGroup(SourceTableGroup.SingleValueNonDietaryExposures);
                }
            }
            progressState.Update(100);
        }
    }
}
