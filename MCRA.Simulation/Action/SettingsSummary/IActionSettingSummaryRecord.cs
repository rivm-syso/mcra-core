namespace MCRA.Simulation.Action {

    public interface IActionSettingSummaryRecord {
        string Option { get; }
        string Value { get; }
        bool IsValid { get; }
    }
}
