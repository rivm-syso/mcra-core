using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;

namespace MCRA.Data.Management.CompiledDataManagers.ActionDataValidators {

    public sealed class CheckMaxSelectionCountValidator : ActionDataReadingValidatorBase {

        public int MaxSelectionCount { get; set; }

        public override DataReadingValidationCheckType CheckType => DataReadingValidationCheckType.CheckMaxSelectionCount;

        public CheckMaxSelectionCountValidator(
            int maxSelectionCount,
            AlertType alertType
        ) : base(alertType) {
            MaxSelectionCount = maxSelectionCount;
        }

        public override IDataReadingValidationResult Validate(
            DataReadingSummaryRecord dataReadingSummaryRecord
        ) {
            if (dataReadingSummaryRecord != null
                && dataReadingSummaryRecord.CodesInScope.Count > MaxSelectionCount
            ) {
                var msg = $"Too many records selected (maximum is {MaxSelectionCount} where {dataReadingSummaryRecord.CodesInScope.Count} are selected).";
                return new DataReadingValidationResult(_alertType, CheckType, msg);
            }
            return null;
        }
    }
}
