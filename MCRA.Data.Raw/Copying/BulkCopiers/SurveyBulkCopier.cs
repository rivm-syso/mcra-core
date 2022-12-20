using MCRA.Utils.DataFileReading;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Raw.Copying.BulkCopiers {
    public sealed class SurveyBulkCopier : IndividualSetBulkCopier {

        public SurveyBulkCopier(
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
            progressState.Update("Processing Surveys");
            var hasSurveys = tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.FoodSurveys);
            if (hasSurveys) {
                progressState.Update("Processing Individuals", 10);
                if (!tryCopyIndividuals(dataSourceReader)) {
                    var msg = "Failed to copy dietary consumption data. Missing individuals data.";
                    throw new RawDataSourceBulkCopyException(msg);
                }
                progressState.Update("Processing Consumptions", 40);
                if (!tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.Consumptions)) {
                    var msg = "Failed to copy dietary consumption data. Missing food consumptions.";
                    throw new RawDataSourceBulkCopyException(msg);
                }
                progressState.Update("Processing IndividualDays", 50);
                tryDoSimpleBulkCopy(dataSourceReader, RawDataSourceTableID.IndividualDays);

                registerTableGroup(SourceTableGroup.Survey);
            }

            progressState.Update(100);
        }
    }
}
