using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;

namespace MCRA.Data.Management.CompiledDataManagers.ActionDataValidators {

    public abstract class ActionDataLinkingValidatorBase : IActionDataLinkingValidator {

        protected string linkingTableReference = "(see linking report)";

        protected AlertType _alertType;

        public ActionDataLinkingValidatorBase(AlertType alertType) {
            _alertType = alertType;
        }

        public abstract DataLinkingValidationCheckType ValidationCheckType { get; }

        public abstract IDataLinkingValidationResult Validate(
            DataLinkingSummaryRecord dataLinkingSummaryRecord
        );
    }
}
