using MCRA.General.SettingsDefinitions;

namespace MCRA.Simulation.Action {

    public interface IActionSettingSummaryRecord {
        SettingsItemType SettingsItemType { get; }
        string Option { get; }
        string Value { get; }
        object RawValue { get; }
        bool IsValid { get; }
    }
}
