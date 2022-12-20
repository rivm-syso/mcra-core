using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.SingleValueConcentrations {
    public class SingleValueConcentrationsSettingsSummarizer : ActionSettingsSummarizerBase {
        public override ActionType ActionType => ActionType.SingleValueConcentrations;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataOrCompute(project, section);
            section.SummarizeSetting(SettingsItemType.UseDeterministicConversionFactors, project.ConcentrationModelSettings.UseDeterministicConversionFactors);
            return section;
        }
    }
}
