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
            section.SummarizeSetting(SettingsItemType.HbmNonDetectsHandlingMethod, _configuration.HbmNonDetectsHandlingMethod);
            section.SummarizeSetting(SettingsItemType.MissingValueImputationMethod, _configuration.MissingValueImputationMethod);
            if (_configuration.MissingValueImputationMethod != MissingValueImputationMethod.NoImputation) {
                section.SummarizeSetting(SettingsItemType.MissingValueCutOff, _configuration.MissingValueCutOff);
            }
            if (_configuration.HbmNonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero) {
                section.SummarizeSetting(SettingsItemType.HbmFractionOfLor, _configuration.HbmFractionOfLor);
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
                    section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, _configuration.TargetDoseLevelType);
                    if (_configuration.TargetDoseLevelType == TargetLevelType.Internal) {
                        section.SummarizeSetting(SettingsItemType.CodeCompartment, _configuration.HbmTargetMatrix, !_configuration.TargetMatrix.IsUndefined());
                    }
                }
            }
            section.SummarizeSetting(SettingsItemType.Cumulative, _configuration.Cumulative);
            section.SummarizeSetting(SettingsItemType.McrAnalysis, _configuration.McrAnalysis);

            if (_configuration.McrAnalysis) {
                section.SummarizeSetting(SettingsItemType.McrExposureApproachType, _configuration.McrExposureApproachType);
                section.SummarizeSetting(SettingsItemType.McrPlotRatioCutOff, _configuration.McrPlotRatioCutOff);
                section.SummarizeSetting(SettingsItemType.McrPlotPercentiles, _configuration.McrPlotPercentiles);
                section.SummarizeSetting(SettingsItemType.McrPlotMinimumPercentage, _configuration.McrPlotMinimumPercentage);
            }
            return section;
        }
    }
}
