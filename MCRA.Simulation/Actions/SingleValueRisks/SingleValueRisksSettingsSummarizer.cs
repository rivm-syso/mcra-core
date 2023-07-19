using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.SingleValueRisks {

    public sealed class SingleValueRisksSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.SingleValueRisks;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.SingleValueRisksCalculationTier, project.EffectModelSettings.SingleValueRisksCalculationTier);
            section.SummarizeSetting(SettingsItemType.SingleValueRiskCalculationMethod, project.EffectModelSettings.SingleValueRiskCalculationMethod);
            if (project.EffectModelSettings.SingleValueRiskCalculationMethod == SingleValueRiskCalculationMethod.FromIndividualRisks) {
                section.SummarizeSetting(SettingsItemType.RiskMetricType, project.EffectModelSettings.RiskMetricType);
                section.SummarizeSetting(SettingsItemType.Percentage, project.EffectModelSettings.Percentage);
                section.SummarizeSetting(SettingsItemType.IsInverseDistribution, project.EffectModelSettings.IsInverseDistribution);
                section.SummarizeSetting(SettingsItemType.HealthEffectType, project.EffectModelSettings.HealthEffectType);
                section.SummarizeSetting(SettingsItemType.UseAdjustmentFactors, project.EffectModelSettings.UseAdjustmentFactors);
                if (project.EffectModelSettings.UseAdjustmentFactors) {
                    if (project.AssessmentSettings.FocalCommodity && project.EffectModelSettings.UseBackgroundAdjustmentFactor) {
                        section.SummarizeSetting(SettingsItemType.ExposureType, project.AssessmentSettings.ExposureType);
                        section.SummarizeSetting(SettingsItemType.FocalCommodityReplacementMethod, project.ConcentrationModelSettings.FocalCommodityReplacementMethod);
                    }
                    if (project.EffectModelSettings.ExposureAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.None) {
                        section.SummarizeSetting(SettingsItemType.ExposureAdjustmentFactorDistributionMethod, project.EffectModelSettings.ExposureAdjustmentFactorDistributionMethod);
                        section.SummarizeSetting(SettingsItemType.ExposureParameterA, project.EffectModelSettings.ExposureParameterA);
                        if (project.EffectModelSettings.ExposureAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.Fixed) {
                            section.SummarizeSetting(SettingsItemType.ExposureParameterB, project.EffectModelSettings.ExposureParameterB);
                            section.SummarizeSetting(SettingsItemType.ExposureParameterC, project.EffectModelSettings.ExposureParameterC);
                            if (project.EffectModelSettings.ExposureAdjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.Beta
                                || project.EffectModelSettings.ExposureAdjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.LogStudents_t) {
                                section.SummarizeSetting(SettingsItemType.ExposureParameterD, project.EffectModelSettings.ExposureParameterD);
                            }
                        }
                    }
                    if (project.EffectModelSettings.HazardAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.None) {
                        section.SummarizeSetting(SettingsItemType.HazardAdjustmentFactorDistributionMethod, project.EffectModelSettings.HazardAdjustmentFactorDistributionMethod);
                        section.SummarizeSetting(SettingsItemType.HazardParameterA, project.EffectModelSettings.HazardParameterA);
                        if (project.EffectModelSettings.HazardAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.Fixed) {
                            section.SummarizeSetting(SettingsItemType.HazardParameterB, project.EffectModelSettings.HazardParameterB);
                            section.SummarizeSetting(SettingsItemType.HazardParameterC, project.EffectModelSettings.HazardParameterC);
                            if (project.EffectModelSettings.HazardAdjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.Beta
                                || project.EffectModelSettings.HazardAdjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.LogStudents_t) {
                                section.SummarizeSetting(SettingsItemType.HazardParameterD, project.EffectModelSettings.HazardParameterD);
                            }
                        }
                    }
                }
                if (project.AssessmentSettings.FocalCommodity) {
                    section.SummarizeSetting(SettingsItemType.UseBackgroundAdjustmentFactor, project.EffectModelSettings.UseBackgroundAdjustmentFactor);
                } else {
                    section.SummarizeSetting(SettingsItemType.UseBackgroundAdjustmentFactor, project.EffectModelSettings.UseBackgroundAdjustmentFactor, isVisible: false);
                }
                section.SummarizeSetting(SettingsItemType.FocalCommodity, project.AssessmentSettings.FocalCommodity);

            }

            if (project.EffectModelSettings.SingleValueRiskCalculationMethod == SingleValueRiskCalculationMethod.FromIndividualRisks
                && project.EffectModelSettings.UseAdjustmentFactors
            ) {
                //These settings are needed in the plots
                section.SummarizeSetting(SettingsItemType.HazardParameterA, project.EffectModelSettings.HazardParameterA, isVisible: false);
                section.SummarizeSetting(SettingsItemType.HazardParameterB, project.EffectModelSettings.HazardParameterB, isVisible: false);
                section.SummarizeSetting(SettingsItemType.HazardParameterC, project.EffectModelSettings.HazardParameterC, isVisible: false);
                section.SummarizeSetting(SettingsItemType.HazardParameterD, project.EffectModelSettings.HazardParameterD, isVisible: false);
                section.SummarizeSetting(SettingsItemType.HazardAdjustmentFactorDistributionMethod, project.EffectModelSettings.HazardAdjustmentFactorDistributionMethod, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureParameterA, project.EffectModelSettings.ExposureParameterA, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureParameterB, project.EffectModelSettings.ExposureParameterB, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureParameterC, project.EffectModelSettings.ExposureParameterC, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureParameterD, project.EffectModelSettings.ExposureParameterD, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureAdjustmentFactorDistributionMethod, project.EffectModelSettings.ExposureAdjustmentFactorDistributionMethod, isVisible: false);
                section.SummarizeSetting(SettingsItemType.UseBackgroundAdjustmentFactor, project.EffectModelSettings.UseBackgroundAdjustmentFactor, isVisible: false);

            }
            return section;
        }
    }
}
