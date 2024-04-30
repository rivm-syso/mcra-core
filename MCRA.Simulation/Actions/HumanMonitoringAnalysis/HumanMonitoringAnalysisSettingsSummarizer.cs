using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {

    public class HumanMonitoringAnalysisSettingsSummarizer : ActionModuleSettingsSummarizer<HumanMonitoringAnalysisModuleConfig> {

        public HumanMonitoringAnalysisSettingsSummarizer(HumanMonitoringAnalysisModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto orih) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.ExposureType, _configuration.ExposureType);
            section.SummarizeSetting(SettingsItemType.NonDetectImputationMethod, _configuration.NonDetectImputationMethod);
            section.SummarizeSetting(SettingsItemType.HumanMonitoringNonDetectsHandlingMethod, _configuration.HumanMonitoringNonDetectsHandlingMethod);
            section.SummarizeSetting(SettingsItemType.MissingValueImputationMethod, _configuration.MissingValueImputationMethod);
            if (_configuration.MissingValueImputationMethod != MissingValueImputationMethod.NoImputation) {
                section.SummarizeSetting(SettingsItemType.MissingValueCutOff, _configuration.MissingValueCutOff);
            }
            if (_configuration.HumanMonitoringNonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero) {
                section.SummarizeSetting(SettingsItemType.HumanMonitoringFractionOfLor, _configuration.HumanMonitoringFractionOfLor);
            }
            section.SummarizeSetting(SettingsItemType.StandardiseBlood, _configuration.StandardiseBlood);
            if (_configuration.StandardiseBlood) {
                section.SummarizeSetting(SettingsItemType.StandardiseBloodMethod, _configuration.StandardiseBloodMethod);
            }
            section.SummarizeSetting(SettingsItemType.StandardiseUrine, _configuration.StandardiseUrine);
            if (_configuration.StandardiseUrine) {
                section.SummarizeSetting(SettingsItemType.StandardiseUrineMethod, _configuration.StandardiseUrineMethod);
                if (_configuration.StandardiseUrineMethod == StandardiseUrineMethod.SpecificGravityCreatinineAdjustment) {
                    section.SummarizeSetting(SettingsItemType.SpecificGravityConversionFactor, _configuration.SpecificGravityConversionFactor);
                }
            }
            section.SummarizeSetting(SettingsItemType.ApplyExposureBiomarkerConversions, _configuration.ApplyExposureBiomarkerConversions);
            section.SummarizeSetting(SettingsItemType.ApplyKineticConversions, _configuration.ApplyKineticConversions);
            if (_configuration.ApplyKineticConversions) {
                section.SummarizeSetting(SettingsItemType.HbmConvertToSingleTargetMatrix, _configuration.HbmConvertToSingleTargetMatrix);
                if (_configuration.HbmConvertToSingleTargetMatrix) {
                    section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, _configuration.HbmTargetSurfaceLevel);
                    if (_configuration.HbmTargetSurfaceLevel == TargetLevelType.Internal) {
                        section.SummarizeSetting(SettingsItemType.CodeCompartment, _configuration.HbmTargetMatrix, !_configuration.TargetMatrix.IsUndefined());
                    }
                }
            }
            section.SummarizeSetting(SettingsItemType.Cumulative, _configuration.Cumulative);
            section.SummarizeSetting(SettingsItemType.AnalyseMcr, _configuration.AnalyseMcr);

            if (_configuration.AnalyseMcr) {
                section.SummarizeSetting(SettingsItemType.ExposureApproachType, _configuration.ExposureApproachType);
                section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioCutOff, _configuration.MaximumCumulativeRatioCutOff);
                section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioPercentiles, _configuration.MaximumCumulativeRatioPercentiles);
                section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioMinimumPercentage, _configuration.MaximumCumulativeRatioMinimumPercentage);
            }
            return section;
        }
    }
}
