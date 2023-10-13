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
            section.SummarizeSetting(SettingsItemType.SingleValueRisksCalculationTier, project.RisksSettings.SingleValueRisksCalculationTier);
            section.SummarizeSetting(SettingsItemType.SingleValueRiskCalculationMethod, project.RisksSettings.SingleValueRiskCalculationMethod);
            if (project.RisksSettings.SingleValueRiskCalculationMethod == SingleValueRiskCalculationMethod.FromIndividualRisks) {
                section.SummarizeSetting(SettingsItemType.RiskMetricType, project.RisksSettings.RiskMetricType);
                section.SummarizeSetting(SettingsItemType.Percentage, project.RisksSettings.Percentage);
                section.SummarizeSetting(SettingsItemType.IsInverseDistribution, project.RisksSettings.IsInverseDistribution);
                section.SummarizeSetting(SettingsItemType.HealthEffectType, project.RisksSettings.HealthEffectType);
                section.SummarizeSetting(SettingsItemType.UseAdjustmentFactors, project.RisksSettings.UseAdjustmentFactors);
                if (project.RisksSettings.UseAdjustmentFactors) {
                    if (project.AssessmentSettings.FocalCommodity && project.RisksSettings.UseBackgroundAdjustmentFactor) {
                        section.SummarizeSetting(SettingsItemType.ExposureType, project.AssessmentSettings.ExposureType);
                        section.SummarizeSetting(SettingsItemType.FocalCommodityReplacementMethod, project.ConcentrationModelSettings.FocalCommodityReplacementMethod);
                    }
                    if (project.RisksSettings.ExposureAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.None) {
                        section.SummarizeSetting(SettingsItemType.ExposureAdjustmentFactorDistributionMethod, project.RisksSettings.ExposureAdjustmentFactorDistributionMethod);
                        section.SummarizeSetting(SettingsItemType.ExposureParameterA, project.RisksSettings.ExposureParameterA);
                        if (project.RisksSettings.ExposureAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.Fixed) {
                            section.SummarizeSetting(SettingsItemType.ExposureParameterB, project.RisksSettings.ExposureParameterB);
                            section.SummarizeSetting(SettingsItemType.ExposureParameterC, project.RisksSettings.ExposureParameterC);
                            if (project.RisksSettings.ExposureAdjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.Beta
                                || project.RisksSettings.ExposureAdjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.LogStudents_t) {
                                section.SummarizeSetting(SettingsItemType.ExposureParameterD, project.RisksSettings.ExposureParameterD);
                            }
                        }
                    }
                    if (project.RisksSettings.HazardAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.None) {
                        section.SummarizeSetting(SettingsItemType.HazardAdjustmentFactorDistributionMethod, project.RisksSettings.HazardAdjustmentFactorDistributionMethod);
                        section.SummarizeSetting(SettingsItemType.HazardParameterA, project.RisksSettings.HazardParameterA);
                        if (project.RisksSettings.HazardAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.Fixed) {
                            section.SummarizeSetting(SettingsItemType.HazardParameterB, project.RisksSettings.HazardParameterB);
                            section.SummarizeSetting(SettingsItemType.HazardParameterC, project.RisksSettings.HazardParameterC);
                            if (project.RisksSettings.HazardAdjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.Beta
                                || project.RisksSettings.HazardAdjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.LogStudents_t) {
                                section.SummarizeSetting(SettingsItemType.HazardParameterD, project.RisksSettings.HazardParameterD);
                            }
                        }
                    }
                }
                if (project.AssessmentSettings.FocalCommodity) {
                    section.SummarizeSetting(SettingsItemType.UseBackgroundAdjustmentFactor, project.RisksSettings.UseBackgroundAdjustmentFactor);
                } else {
                    section.SummarizeSetting(SettingsItemType.UseBackgroundAdjustmentFactor, project.RisksSettings.UseBackgroundAdjustmentFactor, isVisible: false);
                }
                section.SummarizeSetting(SettingsItemType.FocalCommodity, project.AssessmentSettings.FocalCommodity);

            }

            if (project.RisksSettings.SingleValueRiskCalculationMethod == SingleValueRiskCalculationMethod.FromIndividualRisks
                && project.RisksSettings.UseAdjustmentFactors
            ) {
                //These settings are needed in the plots
                section.SummarizeSetting(SettingsItemType.HazardParameterA, project.RisksSettings.HazardParameterA, isVisible: false);
                section.SummarizeSetting(SettingsItemType.HazardParameterB, project.RisksSettings.HazardParameterB, isVisible: false);
                section.SummarizeSetting(SettingsItemType.HazardParameterC, project.RisksSettings.HazardParameterC, isVisible: false);
                section.SummarizeSetting(SettingsItemType.HazardParameterD, project.RisksSettings.HazardParameterD, isVisible: false);
                section.SummarizeSetting(SettingsItemType.HazardAdjustmentFactorDistributionMethod, project.RisksSettings.HazardAdjustmentFactorDistributionMethod, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureParameterA, project.RisksSettings.ExposureParameterA, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureParameterB, project.RisksSettings.ExposureParameterB, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureParameterC, project.RisksSettings.ExposureParameterC, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureParameterD, project.RisksSettings.ExposureParameterD, isVisible: false);
                section.SummarizeSetting(SettingsItemType.ExposureAdjustmentFactorDistributionMethod, project.RisksSettings.ExposureAdjustmentFactorDistributionMethod, isVisible: false);
                section.SummarizeSetting(SettingsItemType.UseBackgroundAdjustmentFactor, project.RisksSettings.UseBackgroundAdjustmentFactor, isVisible: false);

            }
            return section;
        }
    }
}
