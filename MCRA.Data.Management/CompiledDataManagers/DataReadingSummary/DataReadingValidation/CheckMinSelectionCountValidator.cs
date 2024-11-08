using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;

namespace MCRA.Data.Management.CompiledDataManagers.ActionDataValidators {

    public abstract class CheckMinSelectionCountValidator : ActionDataReadingValidatorBase {

        public int MinSelectionCount { get; set; }

        public override DataReadingValidationCheckType CheckType => DataReadingValidationCheckType.CheckMaxSelectionCount;

        public CheckMinSelectionCountValidator(
            int minSelectionCount,
            AlertType alertType
        ) : base(alertType) {
            MinSelectionCount = minSelectionCount;
        }

        public override IDataReadingValidationResult Validate(
            DataReadingSummaryRecord dataReadingSummaryRecord
        ) {
            if (dataReadingSummaryRecord == null) {
                var selectedCount = dataReadingSummaryRecord.CodesInScope.Count;
                if (selectedCount == 0) {
                    var msg = $"No records selected where at least {MinSelectionCount} need to be selected";
                    return new DataReadingValidationResult(_alertType, CheckType, msg);
                } else if (selectedCount < MinSelectionCount) {
                    var msg = $"At least {MinSelectionCount} need to be selected ({dataReadingSummaryRecord.CodesInScope.Count} were selected)";
                    return new DataReadingValidationResult(_alertType, CheckType, msg);
                }
            }
            return null;
        }
    }
}
