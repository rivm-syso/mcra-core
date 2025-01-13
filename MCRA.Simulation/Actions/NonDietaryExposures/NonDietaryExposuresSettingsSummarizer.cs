using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.NonDietaryExposures {

    public sealed class NonDietaryExposuresSettingsSummarizer : ActionModuleSettingsSummarizer<NonDietaryExposuresModuleConfig> {

        public NonDietaryExposuresSettingsSummarizer(NonDietaryExposuresModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.IsCorrelationBetweenIndividuals, _configuration.IsCorrelationBetweenIndividuals);
            return section;
        }
    }
}

