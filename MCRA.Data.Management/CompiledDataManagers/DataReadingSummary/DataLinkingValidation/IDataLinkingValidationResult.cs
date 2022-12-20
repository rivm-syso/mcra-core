using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;

namespace MCRA.Data.Management.CompiledDataManagers.ActionDataValidators {

    public enum DataLinkingValidationCheckType {
        CheckMissingData
    }

    public interface IDataLinkingValidationResult : IDataValidationResult {
        DataLinkingValidationCheckType CheckType { get; }
    }
}
