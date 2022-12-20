using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;

namespace MCRA.Data.Management.CompiledDataManagers.ActionDataValidators {

    public abstract class ActionDataReadingValidatorBase : IActionDataReadingValidator {

        protected string linkingTableReference = "(see linking report)";

        protected AlertType _alertType;

        public ActionDataReadingValidatorBase(AlertType alertType) {
            _alertType = alertType;
        }

        public abstract DataReadingValidationCheckType CheckType { get; }

        public abstract IDataReadingValidationResult Validate(
            DataReadingSummaryRecord dataReadingSummaryRecord
        );
    }
}
