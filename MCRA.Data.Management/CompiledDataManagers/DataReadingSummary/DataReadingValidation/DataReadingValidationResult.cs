using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;

namespace MCRA.Data.Management.CompiledDataManagers.ActionDataValidators {

    public class DataReadingValidationResult : IDataReadingValidationResult {

        public DataReadingValidationResult() { }

        public DataReadingValidationResult(
            AlertType recordType,
            DataReadingValidationCheckType errorCode,
            string message
        ) {
            AlertType = recordType;
            CheckType = errorCode;
            Message = !string.IsNullOrEmpty(message) ? message : errorCode.GetDisplayName();
        }

        public string Message { get; set; }
        public AlertType AlertType { get; set; }
        public DataReadingValidationCheckType CheckType { get; set; }

    }
}
