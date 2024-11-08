using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General.ScopingTypeDefinitions;

namespace MCRA.Data.Management.CompiledDataManagers.ActionDataValidators {

    public sealed class CheckMissingDataValidator : ActionDataLinkingValidatorBase {

        public override DataLinkingValidationCheckType ValidationCheckType => DataLinkingValidationCheckType.CheckMissingData;

        public CheckMissingDataValidator(AlertType alertType)
            : base(alertType) {
        }

        public override IDataLinkingValidationResult Validate(
            DataLinkingSummaryRecord dataLinkingSummaryRecord
        ) {
            if (dataLinkingSummaryRecord == null
                || dataLinkingSummaryRecord.CodesInScopeNotInSource.Any()
            ) {
                var sourceTableName = McraScopingTypeDefinitions.Instance.ScopingDefinitions[dataLinkingSummaryRecord.ScopingType].Name;
                var targetTableName = McraScopingTypeDefinitions.Instance.ScopingDefinitions[dataLinkingSummaryRecord.ReferencedScopingType].Name;
                var count = dataLinkingSummaryRecord.CodesInScopeNotInSource.Count;
                var messageBase = $"There is no data record in the {sourceTableName} table of this data source for";
                if (count == 1) {
                    var undefinedCode = dataLinkingSummaryRecord.CodesInScopeNotInSource.First();
                    var msg = $"{messageBase} the {targetTableName} code {undefinedCode}.";
                    return new DataLinkingValidationResult(_alertType, ValidationCheckType, msg);
                } else if (count <= 3) {
                    var undefinedCodes = string.Join(",", dataLinkingSummaryRecord.CodesInScopeNotInSource);
                    var msg = $"{messageBase} the {targetTableName} codes {undefinedCodes}.";
                    return new DataLinkingValidationResult(_alertType, ValidationCheckType, msg);
                } else {
                    var msg = $"{count} {targetTableName} in the selected {targetTableName} scope are not in the {sourceTableName} table.";
                    return new DataLinkingValidationResult(_alertType, ValidationCheckType, msg);
                }
            }
            return null;
        }
    }
}
