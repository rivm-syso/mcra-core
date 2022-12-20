using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;

namespace MCRA.Data.Management.CompiledDataManagers.ActionDataValidators {

    public class DataLinkingValidationResult : IDataLinkingValidationResult {

        public DataLinkingValidationResult() { }

        public DataLinkingValidationResult(
            AlertType recordType,
            DataLinkingValidationCheckType errorCode,
            string message
        ) {
            AlertType = recordType;
            CheckType = errorCode;
            Message = !string.IsNullOrEmpty(message) ? message : errorCode.GetDisplayName();
        }

        public string Message { get; set; }
        public AlertType AlertType { get; set; }
        public DataLinkingValidationCheckType CheckType { get; set; }
    }
}
