using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions;
using MCRA.General.SettingsDefinitions;

namespace MCRA.Simulation.Action {

    public sealed class ActionMapping {

        public ProjectDto Project { get; set; }
        public ActionType MainActionType { get; set; }
        public ModuleDefinition ModuleDefinition { get; set; }

        // Filled after create
        public Dictionary<ActionType, ActionModuleMapping> ModuleMappingsDictionary { get; set; }
        public HashSet<SettingsItemType> AvailableUncertaintySources { get; set; }
        public HashSet<SettingsItemType> OutputSettings { get; set; }

        public IDictionary<SourceTableGroup, List<int>> GetTableGroupMappings() {
            return ModuleMappingsDictionary.Values
                .Where(r => r.TableGroup != SourceTableGroup.Unknown)
                .Where(r => r.IsVisible)
                .ToDictionary(r => r.TableGroup, r => r.RawDataSources);
        }

        public List<ActionModuleMapping> GetModuleMappings() {
            var result = ModuleMappingsDictionary.Values
                .OrderByDescending(r => r.ModuleDefinition.ModuleType == ModuleType.PrimaryEntityModule)
                .ThenBy(r => r.Order)
                .ToList();
            return result;
        }
    }
}
