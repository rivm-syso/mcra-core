using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.Risks {

    public sealed class RisksSettingsSummarizer : ActionModuleSettingsSummarizer<RisksModuleConfig> {

        public RisksSettingsSummarizer(RisksModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto prot) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.RiskCalculationTier, _configuration.RiskCalculationTier);
            section.SummarizeSetting(SettingsItemType.ExposureType, _configuration.ExposureType);
            section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, _configuration.TargetDoseLevelType);
            section.SummarizeSetting(SettingsItemType.HealthEffectType, _configuration.HealthEffectType);
            section.SummarizeSetting(SettingsItemType.RiskMetricType, _configuration.RiskMetricType);
            section.SummarizeSetting(SettingsItemType.ThresholdMarginOfExposure, _configuration.ThresholdMarginOfExposure);
            section.SummarizeSetting(SettingsItemType.MultipleSubstances, _configuration.MultipleSubstances);
            if (_configuration.MultipleSubstances) {
                section.SummarizeSetting(SettingsItemType.RiskMetricCalculationType, _configuration.RiskMetricCalculationType);
                section.SummarizeSetting(SettingsItemType.Cumulative, _configuration.CumulativeRisk);
            }
            section.SummarizeSetting(SettingsItemType.ConfidenceInterval, _configuration.ConfidenceInterval);
            section.SummarizeSetting(SettingsItemType.IsInverseDistribution, _configuration.IsInverseDistribution);
            section.SummarizeSetting(SettingsItemType.UseIntraSpeciesConversionFactors, _configuration.UseIntraSpeciesConversionFactors, isVisible: _configuration.UseIntraSpeciesConversionFactors);
            section.SummarizeSetting(SettingsItemType.IsEAD, _configuration.IsEAD, isVisible: _configuration.IsEAD);
            section.SummarizeSetting(SettingsItemType.NumberOfLabels, _configuration.NumberOfLabels);
            section.SummarizeSetting(SettingsItemType.NumberOfSubstances, _configuration.NumberOfSubstances);
            section.SummarizeSetting(SettingsItemType.LeftMargin, _configuration.LeftMargin);
            section.SummarizeSetting(SettingsItemType.RightMargin, _configuration.RightMargin);
            if (_configuration.TargetDoseLevelType== TargetLevelType.External) {
                section.SummarizeSetting(SettingsItemType.CalculateRisksByFood, _configuration.CalculateRisksByFood);
            }
            section.SummarizeSetting(SettingsItemType.ExposureCalculationMethod, _configuration.ExposureCalculationMethod);
            if (_configuration.ExposureCalculationMethod == ExposureCalculationMethod.MonitoringConcentration) {
                section.SummarizeSetting(SettingsItemType.CodesHumanMonitoringSamplingMethods, string.Join(",", _configuration.CodesHumanMonitoringSamplingMethods));
            }
            return section;
        }
    }
}
