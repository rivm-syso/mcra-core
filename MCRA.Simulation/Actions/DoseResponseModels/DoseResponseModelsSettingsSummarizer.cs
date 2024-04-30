using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.DoseResponseModels {
    public sealed class DoseResponseModelsSettingsSummarizer : ActionModuleSettingsSummarizer<DoseResponseModelsModuleConfig> {

        public DoseResponseModelsSettingsSummarizer(DoseResponseModelsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.CalculateParametricConfidenceInterval, _configuration.CalculateParametricConfidenceInterval);
            return section;
        }
    }
}
