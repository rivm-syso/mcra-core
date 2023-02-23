using MCRA.General.SettingsDefinitions;

namespace MCRA.Simulation.Action {

    [Serializable]
    public class ActionSettingSummaryRecord : IActionSettingSummaryRecord {
        public SettingsItemType SettingsItemType { get; set; }
        public string Option { get; set; }
        public string Value { get; set; }
        public object RawValue { get; set; }
        public bool IsValid { get; set; }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 31 + Option.GetHashCode();
            hash = hash * 31 + Value.GetHashCode();
            return hash;
        }
    }
}
