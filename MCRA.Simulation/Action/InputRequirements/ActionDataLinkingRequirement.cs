using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Management.CompiledDataManagers.ActionDataValidators;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Simulation.Action {

    public sealed class ActionDataLinkingRequirement {

        public ActionDataLinkingRequirement() {
        }

        public ScopingType SourceScopingType { get; set; }
        public ScopingType TargetScopingType { get; set; }
        public AlertType AlertTypeMissingData { get; set; } = AlertType.Error;

        public ICollection<IDataValidationResult> Validate(DataLinkingSummaryRecord dataLinkingSummaryRecord) {
            var result = new List<IDataValidationResult>();
            var validators = createValidators();
            foreach (var validator in validators) {
                var validationResult = validator.Validate(dataLinkingSummaryRecord);
                if (validationResult != null) {
                    result.Add(validationResult);
                }
            }
            return result;
        }

        private ICollection<IActionDataLinkingValidator> createValidators() {
            var validators = new List<IActionDataLinkingValidator>();
            if (AlertTypeMissingData != AlertType.None) {
                validators.Add(new CheckMissingDataValidator(AlertTypeMissingData));
            }
            return validators;
        }
    }
}
