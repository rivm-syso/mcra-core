using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.InterSpeciesConversions {

    public sealed class InterSpeciesConversionsSettingsSummarizer : ActionModuleSettingsSummarizer<InterSpeciesConversionsModuleConfig> {

        public InterSpeciesConversionsSettingsSummarizer(InterSpeciesConversionsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.DefaultInterSpeciesFactorGeometricMean, _configuration.DefaultInterSpeciesFactorGeometricMean);
            section.SummarizeSetting(SettingsItemType.DefaultInterSpeciesFactorGeometricStandardDeviation, _configuration.DefaultInterSpeciesFactorGeometricStandardDeviation);
            return section;
        }
    }
}
