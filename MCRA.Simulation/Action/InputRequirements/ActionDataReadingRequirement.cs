using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Management.CompiledDataManagers.ActionDataValidators;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Simulation.Action {

    public sealed class ActionDataReadingRequirement {

        public ActionDataReadingRequirement() {
        }

        public ActionDataReadingRequirement(
            ScopingType sourceTableId,
            bool allowCodesInScopeNotInSource = false,
            bool allowEmptyScope = false,
            int maxSelectionCount = -1,
            int minSelectionCount = -1
        ) {
            SourceScopingType = sourceTableId;
            MinSelectionCount = minSelectionCount;
            MaxSelectionCount = maxSelectionCount;
            AllowEmptyScope = allowEmptyScope;
            AllowCodesInScopeNotInSource = allowCodesInScopeNotInSource;
        }

        public ScopingType SourceScopingType { get; set; }
        public bool AllowCodesInScopeNotInSource { get; set; } = false;
        public bool AllowEmptyScope { get; set; } = false;
        public int MinSelectionCount { get; set; } = -1;
        public int MaxSelectionCount { get; set; } = -1;

        public ICollection<IDataValidationResult> Validate(DataReadingSummaryRecord dataReadingSummaryRecord) {
            var result = new List<IDataValidationResult>();
            var validators = createValidators();
            foreach (var validator in validators) {
                var validationResult = validator.Validate(dataReadingSummaryRecord);
                if (validationResult != null) {
                    result.Add(validationResult);
                }
            }
            return result;
        }

        private ICollection<IActionDataReadingValidator> createValidators() {
            var validators = new List<IActionDataReadingValidator>();
            if (!AllowEmptyScope) {
                validators.Add(new CheckNoSelectionValidator(AlertType.Error));
            }
            if (MaxSelectionCount > -1) {
                validators.Add(new CheckMaxSelectionCountValidator(MaxSelectionCount, AlertType.Error));
            }
            if (MinSelectionCount > -1) {
                validators.Add(new CheckMaxSelectionCountValidator(MinSelectionCount, AlertType.Error));
            }
            if (!AllowCodesInScopeNotInSource) {
                validators.Add(new CheckUndefinedReferencesValidator(AlertType.Error));
            } else {
                validators.Add(new CheckUndefinedReferencesValidator(AlertType.Notification));
            }
            return validators;
        }
    }
}
