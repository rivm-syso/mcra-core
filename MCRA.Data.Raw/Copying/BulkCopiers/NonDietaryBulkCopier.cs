using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class NonDietaryBulkCopier : RawDataSourceBulkCopierBase {

        public NonDietaryBulkCopier(
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
            progressState.Update("Processing non-dietary exposure data");
            var hasSurveys = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.NonDietarySurveys);
            if(hasSurveys) {
                if(!tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.NonDietaryExposures)) {
                    var msg = "Failed to copy non dietary exposures data. Missing non dietary exposures.";
                    throw new RawDataSourceBulkCopyException(msg);
                }
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.NonDietaryExposuresUncertain);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.NonDietarySurveyProperties);

                registerTableGroup(SourceTableGroup.NonDietary);
            }
            progressState.Update(100);
        }
    }
}
