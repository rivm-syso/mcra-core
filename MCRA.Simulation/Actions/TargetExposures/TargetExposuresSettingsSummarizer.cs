using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.TargetExposures {

    public sealed class TargetExposuresSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.TargetExposures;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.ExposureType, project.AssessmentSettings.ExposureType);
            section.SummarizeSetting(SettingsItemType.Aggregate, project.AssessmentSettings.Aggregate);
            if (project.AssessmentSettings.Aggregate) {
                section.SummarizeSetting(SettingsItemType.MatchSpecificIndividuals, project.NonDietarySettings.MatchSpecificIndividuals);
                if (!project.NonDietarySettings.MatchSpecificIndividuals) {
                    section.SummarizeSetting(SettingsItemType.IsCorrelationBetweenIndividuals, project.NonDietarySettings.IsCorrelationBetweenIndividuals);
                }
            }
            section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, project.EffectSettings.TargetDoseLevelType);
            if (project.AssessmentSettings.ExposureType == ExposureType.Chronic) {
                section.SummarizeSetting(SettingsItemType.IntakeModelType, project.IntakeModelSettings.IntakeModelType);
            }
            if (project.IntakeModelSettings.FirstModelThenAdd) {
                section.SummarizeSetting(SettingsItemType.FirstModelThenAdd, project.IntakeModelSettings.FirstModelThenAdd);
            }
            section.SummarizeSetting(SettingsItemType.AnalyseMcr, project.EffectSettings.AnalyseMcr);
            if (project.EffectSettings.AnalyseMcr) {
                section.SummarizeSetting(SettingsItemType.ExposureApproachType, project.EffectSettings.ExposureApproachType);
                section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioCutOff, project.OutputDetailSettings.MaximumCumulativeRatioCutOff);
                section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioPercentiles, project.OutputDetailSettings.MaximumCumulativeRatioPercentiles);
                section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioMinimumPercentage, project.OutputDetailSettings.MaximumCumulativeRatioMinimumPercentage);
            }
            return section;
        }
    }
}

