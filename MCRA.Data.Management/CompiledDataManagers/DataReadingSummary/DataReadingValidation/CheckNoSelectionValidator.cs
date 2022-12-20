using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;

namespace MCRA.Data.Management.CompiledDataManagers.ActionDataValidators {

    public sealed class CheckNoSelectionValidator : ActionDataReadingValidatorBase {

        public override DataReadingValidationCheckType CheckType => DataReadingValidationCheckType.CheckNoSelection;

        public CheckNoSelectionValidator(AlertType alertType) 
            : base(alertType) {
        }

        public override IDataReadingValidationResult Validate(
            DataReadingSummaryRecord dataReadingSummaryRecord
        ) {
            if (dataReadingSummaryRecord == null
                || dataReadingSummaryRecord.CodesInScope.Count == 0
            ) {
                var msg = $"No records were selected";
                return new DataReadingValidationResult(_alertType, CheckType, msg);
            }
            return null;
        }
    }
}
