using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.HumanMonitoringAnalysis {

    public class HumanMonitoringAnalysisSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.HumanMonitoringAnalysis;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var hms = project.HumanMonitoringSettings;
            section.SummarizeSetting(SettingsItemType.ExposureType, project.AssessmentSettings.ExposureType);
            section.SummarizeSetting(SettingsItemType.NonDetectImputationMethod, hms.NonDetectImputationMethod);
            section.SummarizeSetting(SettingsItemType.HumanMonitoringNonDetectsHandlingMethod, hms.NonDetectsHandlingMethod);
            section.SummarizeSetting(SettingsItemType.MissingValueImputationMethod, hms.MissingValueImputationMethod);
            if (hms.MissingValueImputationMethod != MissingValueImputationMethod.NoImputation) {
                section.SummarizeSetting(SettingsItemType.MissingValueCutOff, hms.MissingValueCutOff);
            }
            if (hms.NonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero) {
                section.SummarizeSetting(SettingsItemType.HumanMonitoringFractionOfLor, hms.FractionOfLor);
            }
            section.SummarizeSetting(SettingsItemType.StandardiseBlood, hms.StandardiseBlood);
            if (hms.StandardiseBlood) {
                section.SummarizeSetting(SettingsItemType.StandardiseBloodMethod, hms.StandardiseBloodMethod);
            }
            section.SummarizeSetting(SettingsItemType.StandardiseUrine, hms.StandardiseUrine);
            if (hms.StandardiseUrine) {
                section.SummarizeSetting(SettingsItemType.StandardiseUrineMethod, hms.StandardiseUrineMethod);
                if (hms.StandardiseUrineMethod == StandardiseUrineMethod.SpecificGravityCreatinineAdjustment) {
                    section.SummarizeSetting(SettingsItemType.SpecificGravityConversionFactor, hms.SpecificGravityConversionFactor);
                }
            }
            section.SummarizeSetting(SettingsItemType.ApplyExposureBiomarkerConversions, hms.ApplyExposureBiomarkerConversions);
            section.SummarizeSetting(SettingsItemType.ApplyKineticConversions, hms.ApplyKineticConversions);
            if (hms.ApplyKineticConversions) {
                section.SummarizeSetting(SettingsItemType.HbmConvertToSingleTargetMatrix, hms.HbmConvertToSingleTargetMatrix);
                if (hms.HbmConvertToSingleTargetMatrix) {
                    section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, hms.HbmTargetSurfaceLevel);
                    if (hms.HbmTargetSurfaceLevel == TargetLevelType.Internal) {
                        section.SummarizeSetting(SettingsItemType.CodeCompartment, hms.HbmTargetMatrix, !hms.TargetMatrix.IsUndefined());
                    }
                }
            }
            section.SummarizeSetting(SettingsItemType.Cumulative, project.AssessmentSettings.Cumulative);
            section.SummarizeSetting(SettingsItemType.AnalyseMcr, hms.AnalyseMcr);

            if (hms.AnalyseMcr) {
                section.SummarizeSetting(SettingsItemType.ExposureApproachType, hms.ExposureApproachType);
                section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioCutOff, project.OutputDetailSettings.MaximumCumulativeRatioCutOff);
                section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioPercentiles, project.OutputDetailSettings.MaximumCumulativeRatioPercentiles);
                section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioMinimumPercentage, project.OutputDetailSettings.MaximumCumulativeRatioMinimumPercentage);
            }
            return section;
        }
    }
}
