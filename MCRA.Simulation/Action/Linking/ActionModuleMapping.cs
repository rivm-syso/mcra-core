using MCRA.Data.Management.CompiledDataManagers;
using MCRA.General;
using MCRA.General.ModuleDefinitions;

namespace MCRA.Simulation.Action {

    public sealed class ActionModuleMapping {
        public ActionType ActionType { get; set; }
        public ModuleDefinition ModuleDefinition { get; set; }
        public bool IsMainModule { get; set; }
        public SourceTableGroup TableGroup { get; set; }
        public int Order { get; set; }
        public IActionCalculator ActionCalculator { get; set; }
        public bool IsRequired { get; set; }
        public bool IsVisible { get; set; }
        public bool IsCompute { get; set; }
        public bool IsSettingsValid { get; set; }
        public bool IsDataValid { get; set; }
        public bool IsDataDependentSettingsValid { get; set; }
        public bool IsValid { get; set; }
        public List<int> RawDataSources { get; set; }
        public List<ActionInputRequirement> InputRequirements { get; set; }
        public HashSet<ActionType> UsedByModules { get; set; } = new();
        public ActionSettingsSummary Settings { get; set; }
        public Dictionary<ScopingType, DataReadingReport> CompiledDataReadingReports { get; set; }

        public bool IsSpecified {
            get {
                return IsVisible && (IsCompute || RawDataSources.Any());
            }
        }

        public bool HasData {
            get {
                return !IsCompute && (RawDataSources?.Count > 0);
            }
        }

        public bool ExpectsData {
            get {
                return !IsCompute;
            }
        }

        public override string ToString() {
            return $"{ActionType}: Req {(IsRequired ? "Y" : "N")} Vis {(IsVisible ? "Y" : "N")} IsValid {(IsValid ? "Y" : "N")} Order {Order}";
        }
    }
}
