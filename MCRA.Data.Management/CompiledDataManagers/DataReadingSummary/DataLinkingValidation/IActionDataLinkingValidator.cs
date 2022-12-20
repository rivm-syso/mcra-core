namespace MCRA.Data.Management.CompiledDataManagers.ActionDataValidators {

    public interface IActionDataLinkingValidator {
        DataLinkingValidationCheckType ValidationCheckType { get; }
        IDataLinkingValidationResult Validate(DataLinkingSummaryRecord dataLinkingSummaryRecord);
    }
}
