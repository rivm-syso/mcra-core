using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.Risks {

    public sealed class RisksSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.Risks;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var ems = project.EffectModelSettings;
            var es = project.EffectSettings;
            section.SummarizeSetting(SettingsItemType.ExposureType, project.AssessmentSettings.ExposureType);
            section.SummarizeSetting(SettingsItemType.HealthEffectType, ems.HealthEffectType);
            section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, project.EffectSettings.TargetDoseLevelType);
            section.SummarizeSetting(SettingsItemType.RiskMetricType, ems.RiskMetricType);
            section.SummarizeSetting(SettingsItemType.LeftMargin, ems.LeftMargin);
            section.SummarizeSetting(SettingsItemType.RightMargin, ems.RightMargin);
            section.SummarizeSetting(SettingsItemType.UseIntraSpeciesConversionFactors, es.UseIntraSpeciesConversionFactors, isVisible: es.UseIntraSpeciesConversionFactors);
            section.SummarizeSetting(SettingsItemType.IsEAD, ems.IsEAD, isVisible: ems.IsEAD);
            section.SummarizeSetting(SettingsItemType.ThresholdMarginOfExposure, ems.ThresholdMarginOfExposure);
            section.SummarizeSetting(SettingsItemType.ConfidenceInterval, ems.ConfidenceInterval);
            section.SummarizeSetting(SettingsItemType.NumberOfLabels, ems.NumberOfLabels);
            section.SummarizeSetting(SettingsItemType.NumberOfSubstances, ems.NumberOfSubstances);
            section.SummarizeSetting(SettingsItemType.IsInverseDistribution, ems.IsInverseDistribution);
            section.SummarizeSetting(SettingsItemType.MultipleSubstances, project.AssessmentSettings.MultipleSubstances);
            if (project.AssessmentSettings.MultipleSubstances) {
                section.SummarizeSetting(SettingsItemType.Cumulative, project.AssessmentSettings.Cumulative);
            }
            if (project.EffectSettings.TargetDoseLevelType== TargetLevelType.External) {
                section.SummarizeSetting(SettingsItemType.CalculateRisksByFood, ems.CalculateRisksByFood);
            }
            return section;
        }
    }
}
