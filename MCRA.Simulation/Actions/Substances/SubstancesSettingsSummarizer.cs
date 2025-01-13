using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.Substances {

    public class SubstancesSettingsSummarizer : ActionModuleSettingsSummarizer<SubstancesModuleConfig> {

        public SubstancesSettingsSummarizer(SubstancesModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataSources(project, section);
            if (_configuration.MultipleSubstances) {
                section.SummarizeSetting(SettingsItemType.MultipleSubstances, _configuration.MultipleSubstances);
            }
            return section;
        }
    }
}
