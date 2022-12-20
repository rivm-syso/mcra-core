using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;

namespace MCRA.Data.Management.CompiledDataManagers.ActionDataValidators {

    public enum DataReadingValidationCheckType {
        CheckUndefinedEntities = 0,
        CheckNoSelection = 10,
        CheckMaxSelectionCount = 11
    }

    public interface IDataReadingValidationResult : IDataValidationResult {
        DataReadingValidationCheckType CheckType { get; }
    }
}
