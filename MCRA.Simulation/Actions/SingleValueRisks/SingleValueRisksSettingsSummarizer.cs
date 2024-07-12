using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.SingleValueRisks {

    public sealed class SingleValueRisksSettingsSummarizer : ActionModuleSettingsSummarizer<SingleValueRisksModuleConfig> {

        public SingleValueRisksSettingsSummarizer(SingleValueRisksModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.SelectedTier, _configuration.SelectedTier);
            section.SummarizeSetting(SettingsItemType.SingleValueRiskCalculationMethod, _configuration.SingleValueRiskCalculationMethod);
            if (_configuration.SingleValueRiskCalculationMethod == SingleValueRiskCalculationMethod.FromIndividualRisks) {
                section.SummarizeSetting(SettingsItemType.RiskMetricType, _configuration.RiskMetricType);
                section.SummarizeSetting(SettingsItemType.Percentage, _configuration.Percentage);
                section.SummarizeSetting(SettingsItemType.IsInverseDistribution, _configuration.IsInverseDistribution);
                section.SummarizeSetting(SettingsItemType.HealthEffectType, _configuration.HealthEffectType);
                section.SummarizeSetting(SettingsItemType.UseAdjustmentFactors, _configuration.UseAdjustmentFactors);
                if (_configuration.UseAdjustmentFactors) {
                    if (_configuration.FocalCommodity && _configuration.UseBackgroundAdjustmentFactor) {
                        section.SummarizeSetting(SettingsItemType.ExposureType, _configuration.ExposureType);
                        section.SummarizeSetting(SettingsItemType.FocalCommodityReplacementMethod, _configuration.FocalCommodityReplacementMethod);
                    }
                    if (_configuration.ExposureAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.None) {
                        section.SummarizeSetting(SettingsItemType.ExposureAdjustmentFactorDistributionMethod, _configuration.ExposureAdjustmentFactorDistributionMethod);
                        section.SummarizeSetting(SettingsItemType.ExposureParameterA, _configuration.ExposureParameterA);
                        if (_configuration.ExposureAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.Fixed) {
                            section.SummarizeSetting(SettingsItemType.ExposureParameterB, _configuration.ExposureParameterB);
                            section.SummarizeSetting(SettingsItemType.ExposureParameterC, _configuration.ExposureParameterC);
                            if (_configuration.ExposureAdjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.Beta
                                || _configuration.ExposureAdjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.LogStudents_t) {
                                section.SummarizeSetting(SettingsItemType.ExposureParameterD, _configuration.ExposureParameterD);
                            }
                        }
                    }
                    if (_configuration.HazardAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.None) {
                        section.SummarizeSetting(SettingsItemType.HazardAdjustmentFactorDistributionMethod, _configuration.HazardAdjustmentFactorDistributionMethod);
                        section.SummarizeSetting(SettingsItemType.HazardParameterA, _configuration.HazardParameterA);
                        if (_configuration.HazardAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.Fixed) {
                            section.SummarizeSetting(SettingsItemType.HazardParameterB, _configuration.HazardParameterB);
                            section.SummarizeSetting(SettingsItemType.HazardParameterC, _configuration.HazardParameterC);
                            if (_configuration.HazardAdjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.Beta
                                || _configuration.HazardAdjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.LogStudents_t) {
                                section.SummarizeSetting(SettingsItemType.HazardParameterD, _configuration.HazardParameterD);
                            }
                        }
                    }
                }
                if (_configuration.FocalCommodity) {
                    section.SummarizeSetting(SettingsItemType.UseBackgroundAdjustmentFactor, _configuration.UseBackgroundAdjustmentFactor);
                } else {
                    section.SummarizeSetting(SettingsItemType.UseBackgroundAdjustmentFactor, _configuration.UseBackgroundAdjustmentFactor, isVisible: false);
                }
                section.SummarizeSetting(SettingsItemType.FocalCommodity, _configuration.FocalCommodity);

            }

            if (_configuration.SingleValueRiskCalculationMethod == SingleValueRiskCalculationMethod.FromIndividualRisks
                && _configuration.UseAdjustmentFactors
            ) {
                //These settings are needed in the plots
                section.SummarizeSetting(SettingsItemType.HazardParameterA, _configuration.HazardParameterA, isVisible: false);
                section.SummarizeSetting(SettingsItemType.HazardParameterB, _configuration.HazardParameterB, isVisible: false);
                section.SummarizeSetting(SettingsItemType.HazardParameterC, _configuration.HazardParameterC, isVisible: false);
                section.SummarizeSetting(SettingsItemType.HazardParameterD, _configuration.HazardParameterD, isVisible: false);
                section.SummarizeSetting(SettingsItemType.HazardAdjustmentFactorDistributionMethod, _configuration.HazardAdjustmentFactorDistributionMethod, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureParameterA, _configuration.ExposureParameterA, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureParameterB, _configuration.ExposureParameterB, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureParameterC, _configuration.ExposureParameterC, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureParameterD, _configuration.ExposureParameterD, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureAdjustmentFactorDistributionMethod, _configuration.ExposureAdjustmentFactorDistributionMethod, isVisible: false);
                section.SummarizeSetting(SettingsItemType.UseBackgroundAdjustmentFactor, _configuration.UseBackgroundAdjustmentFactor, isVisible: false);

            }
            return section;
        }
    }
}
