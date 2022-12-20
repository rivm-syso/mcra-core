namespace MCRA.Data.Management.CompiledDataManagers.DataReadingSummary {

    public enum AlertType {
        None = -1,
        Notification = 0,
        Warning = 1,
        Error = 2
    }

    public interface IDataValidationResult {
        AlertType AlertType { get; }
        string Message { get; }
    }
}
