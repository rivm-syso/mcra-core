using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ExposureMixtures {

    public sealed class ExposureMixturesSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.ExposureMixtures;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var ms = project.MixtureSelectionSettings;
            section.SummarizeSetting(SettingsItemType.ExposureType, project.AssessmentSettings.ExposureType, isVisible: false);
            section.SummarizeSetting(SettingsItemType.MixtureSelectionSparsenessConstraint, ms.SW);
            section.SummarizeSetting(SettingsItemType.NumberOfMixtures, ms.K);
            section.SummarizeSetting(SettingsItemType.MixtureSelectionIterations, ms.NumberOfIterations);
            section.SummarizeSetting(SettingsItemType.ExposureApproachType, ms.ExposureApproachType);
            section.SummarizeSetting(SettingsItemType.MixtureSelectionConvergenceCriterium, ms.Epsilon);
            section.SummarizeSetting(SettingsItemType.MixtureSelectionRatioCutOff, ms.RatioCutOff);
            section.SummarizeSetting(SettingsItemType.MixtureSelectionTotalExposureCutOff, ms.TotalExposureCutOff);
            section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, project.EffectSettings.TargetDoseLevelType);
            section.SummarizeSetting(SettingsItemType.ClusterMethodType, ms.ClusterMethodType);
            if (ms.ClusterMethodType != ClusterMethodType.NoClustering) {
                section.SummarizeSetting(SettingsItemType.NumberOfClusters, ms.NumberOfClusters);
                if (ms.ClusterMethodType == ClusterMethodType.Hierarchical) {
                    section.SummarizeSetting(SettingsItemType.AutomaticallyDeterminationOfClusters, ms.AutomaticallyDeterminationOfClusters);
                }
            }
            if (project.EffectSettings.TargetDoseLevelType == TargetLevelType.Internal) {
                section.SummarizeSetting(SettingsItemType.InternalConcentrationType, project.AssessmentSettings.InternalConcentrationType);
                if (project.AssessmentSettings.InternalConcentrationType == InternalConcentrationType.MonitoringConcentration) {
                    section.SummarizeSetting(SettingsItemType.CodesHumanMonitoringSamplingMethods, string.Join(",", project.HumanMonitoringSettings.SamplingMethodCodes.Select(r => r)));
                }
            }
            section.SummarizeSetting(SettingsItemType.NetworkAnalysisType, ms.NetworkAnalysisType);
            if (ms.NetworkAnalysisType != NetworkAnalysisType.NoNetworkAnalysis) {
                section.SummarizeSetting(SettingsItemType.IsLogTransform, ms.IsLogTransform);
            }
            return section;
        }
    }
}
