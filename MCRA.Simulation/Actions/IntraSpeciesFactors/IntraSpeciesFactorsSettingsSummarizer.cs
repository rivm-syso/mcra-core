using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.IntraSpeciesFactors {

    public sealed class IntraSpeciesFactorsSettingsSummarizer : ActionModuleSettingsSummarizer<IntraSpeciesFactorsModuleConfig> {

        public IntraSpeciesFactorsSettingsSummarizer(IntraSpeciesFactorsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.DefaultIntraSpeciesFactor, _configuration.DefaultIntraSpeciesFactor);
            return section;
        }
    }
}
