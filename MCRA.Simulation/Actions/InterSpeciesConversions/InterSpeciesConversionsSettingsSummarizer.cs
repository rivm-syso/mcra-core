using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.InterSpeciesConversions {

    public sealed class InterSpeciesConversionsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.InterSpeciesConversions;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var ems = project.EffectModelSettings;
            section.SummarizeSetting(SettingsItemType.DefaultInterSpeciesFactorGeometricMean, ems.DefaultInterSpeciesFactorGeometricMean);
            section.SummarizeSetting(SettingsItemType.DefaultInterSpeciesFactorGeometricStandardDeviation, ems.DefaultInterSpeciesFactorGeometricStandardDeviation);
            return section;
        }
    }
}
