using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.SingleValueDietaryExposures {

    public class SingleValueDietaryExposuresSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.SingleValueDietaryExposures;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.SingleValueDietaryExposureCalculationMethod, project.DietaryIntakeCalculationSettings.SingleValueDietaryExposureCalculationMethod);
            if (project.AssessmentSettings.ExposureType == ExposureType.Acute) {
                section.SummarizeSetting("Apply unit variability", project.UnitVariabilitySettings.UseUnitVariability);
            } else {
                section.SummarizeSetting(SettingsItemType.UseUnitVariability, project.UnitVariabilitySettings.UseUnitVariability, isVisible: false);
            }
            return section;
        }
    }
}
