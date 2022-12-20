using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {

    public class HumanMonitoringAnalysisSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.HumanMonitoringAnalysis;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var hms = project.HumanMonitoringSettings;
            section.SummarizeSetting(SettingsItemType.ExposureType, project.AssessmentSettings.ExposureType);
            section.SummarizeSetting(SettingsItemType.HumanMonitoringNonDetectsHandlingMethod, hms.NonDetectsHandlingMethod);
            if (hms.NonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLOR) {
                section.SummarizeSetting(SettingsItemType.HumanMonitoringFractionOfLor, hms.FractionOfLor);
            }
            section.SummarizeSetting(SettingsItemType.MissingValueImputationMethod, hms.MissingValueImputationMethod);
            section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioCutOff, project.OutputDetailSettings.MaximumCumulativeRatioCutOff);
            section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioPercentiles, project.OutputDetailSettings.MaximumCumulativeRatioPercentiles);
            section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioMinimumPercentage, project.OutputDetailSettings.MaximumCumulativeRatioMinimumPercentage);
            return section;
        }
    }
}
