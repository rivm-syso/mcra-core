using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;

namespace MCRA.Data.Management.CompiledDataManagers.ActionDataValidators {

    public interface IActionDataReadingValidator {
        DataReadingValidationCheckType CheckType { get; }
        IDataReadingValidationResult Validate(DataReadingSummaryRecord dataReadingSummaryRecord);
    }
}
