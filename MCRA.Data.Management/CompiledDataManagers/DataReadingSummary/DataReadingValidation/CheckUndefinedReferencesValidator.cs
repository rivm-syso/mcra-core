using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;

namespace MCRA.Data.Management.CompiledDataManagers.ActionDataValidators {

    public sealed class CheckUndefinedReferencesValidator : ActionDataReadingValidatorBase {

        public override DataReadingValidationCheckType CheckType => DataReadingValidationCheckType.CheckUndefinedEntities;

        public CheckUndefinedReferencesValidator(AlertType alertType)
            : base(alertType) {
        }

        public override IDataReadingValidationResult Validate(
            DataReadingSummaryRecord dataReadingSummaryRecord
        ) {
            if (dataReadingSummaryRecord != null
                && dataReadingSummaryRecord.CodesInScopeNotInSource.Count > 0
            ) {
                var missingDataRecordsCount = dataReadingSummaryRecord.CodesInScopeNotInSource.Count;
                var scopeTableName = $"the {dataReadingSummaryRecord.ScopingType.GetDisplayName()} table";
                var messageBase = $"referenced by some other data source, but not present in {scopeTableName} of this data source";
                if (missingDataRecordsCount == 1) {
                    var undefinedCode = dataReadingSummaryRecord.CodesInScopeNotInSource.First();
                    var msg = $"The code {undefinedCode} is {messageBase}.";
                    return new DataReadingValidationResult(_alertType, CheckType, msg);
                } else if (missingDataRecordsCount <= 3) {
                    var undefinedCodes = string.Join(", ", dataReadingSummaryRecord.CodesInScopeNotInSource, 0, missingDataRecordsCount - 1) +
                        $" and {dataReadingSummaryRecord.CodesInScopeNotInSource.Last()}";
                    var msg = $"The codes {undefinedCodes} are {messageBase}.";
                    return new DataReadingValidationResult(_alertType, CheckType, msg);
                } else {
                    var msg = $"There are {missingDataRecordsCount} codes that are {messageBase} {linkingTableReference}.";
                    return new DataReadingValidationResult(_alertType, CheckType, msg);
                }
            }
            return null;
        }
    }
}
