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
            section.SummarizeSetting(SettingsItemType.HumanMonitoringNonDetectsHandlingMethod, hms.NonDetectsHandlingMethod);
            if (hms.NonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero) {
                section.SummarizeSetting(SettingsItemType.HumanMonitoringFractionOfLor, hms.FractionOfLor);
            }
            section.SummarizeSetting(SettingsItemType.MissingValueImputationMethod, hms.MissingValueImputationMethod);
            if (hms.MissingValueImputationMethod != MissingValueImputationMethod.NoImputation) {
                section.SummarizeSetting(SettingsItemType.MissingValueCutOff, hms.MissingValueCutOff);
            }
            section.SummarizeSetting(SettingsItemType.IsMcrAnalysis, project.MixtureSelectionSettings.IsMcrAnalysis);
            if (project.MixtureSelectionSettings.IsMcrAnalysis) {
                section.SummarizeSetting(SettingsItemType.McrExposureApproachType, project.MixtureSelectionSettings.McrExposureApproachType);
                section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioCutOff, project.OutputDetailSettings.MaximumCumulativeRatioCutOff);
                section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioPercentiles, project.OutputDetailSettings.MaximumCumulativeRatioPercentiles);
                section.SummarizeSetting(SettingsItemType.MaximumCumulativeRatioMinimumPercentage, project.OutputDetailSettings.MaximumCumulativeRatioMinimumPercentage);
            }
            section.SummarizeSetting(SettingsItemType.NonDetectImputationMethod, project.HumanMonitoringSettings.NonDetectImputationMethod);
            section.SummarizeSetting(SettingsItemType.CodeCompartment, project.HumanMonitoringSettings.HbmTargetMatrix, !project.HumanMonitoringSettings.TargetMatrix.IsUndefined());

            section.SummarizeSetting(SettingsItemType.ImputeHbmConcentrationsFromOtherMatrices, project.HumanMonitoringSettings.ImputeHbmConcentrationsFromOtherMatrices);
            if (project.HumanMonitoringSettings.ImputeHbmConcentrationsFromOtherMatrices) {
                section.SummarizeSetting(SettingsItemType.HbmBetweenMatrixConversionFactor, project.HumanMonitoringSettings.HbmBetweenMatrixConversionFactor);
            }
            section.SummarizeSetting(SettingsItemType.StandardiseBlood, project.HumanMonitoringSettings.StandardiseBlood);
            if (project.HumanMonitoringSettings.StandardiseBlood) {
                section.SummarizeSetting(SettingsItemType.StandardiseBloodMethod, project.HumanMonitoringSettings.StandardiseBloodMethod);
            }
            section.SummarizeSetting(SettingsItemType.StandardiseUrine, project.HumanMonitoringSettings.StandardiseUrine);
            if (project.HumanMonitoringSettings.StandardiseUrine) {
                section.SummarizeSetting(SettingsItemType.StandardiseUrineMethod, project.HumanMonitoringSettings.StandardiseUrineMethod);
            }
            return section;
        }
    }
}
