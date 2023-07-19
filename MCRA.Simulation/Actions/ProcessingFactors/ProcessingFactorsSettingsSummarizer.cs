using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ProcessingFactors {
    public class ProcessingFactorsSettingsSummarizer : ActionSettingsSummarizerBase {
        public override ActionType ActionType => ActionType.ProcessingFactors;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.UseProcessing, project.ConcentrationModelSettings.IsProcessing, isVisible: false);
            if (project.ConcentrationModelSettings.IsProcessing) {
                section.SummarizeSetting(SettingsItemType.IsDistribution, project.ConcentrationModelSettings.IsDistribution, isVisible: false);
                section.SummarizeSetting(SettingsItemType.AllowHigherThanOne, project.ConcentrationModelSettings.AllowHigherThanOne, isVisible: false);
            }
            return section;
        }
    }
}
