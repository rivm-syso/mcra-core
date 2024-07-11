using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.ExposureMixtures {

    public sealed class ExposureMixturesSettingsSummarizer : ActionModuleSettingsSummarizer<ExposureMixturesModuleConfig> {

        public ExposureMixturesSettingsSummarizer(ExposureMixturesModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto proj = null) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.ExposureType, _configuration.ExposureType, isVisible: false);
            section.SummarizeSetting(SettingsItemType.MixtureSelectionSparsenessConstraint, _configuration.MixtureSelectionSparsenessConstraint);
            section.SummarizeSetting(SettingsItemType.NumberOfMixtures, _configuration.NumberOfMixtures);
            section.SummarizeSetting(SettingsItemType.MixtureSelectionIterations, _configuration.MixtureSelectionIterations);
            section.SummarizeSetting(SettingsItemType.ExposureApproachType, _configuration.ExposureApproachType);
            section.SummarizeSetting(SettingsItemType.MixtureSelectionConvergenceCriterium, _configuration.MixtureSelectionConvergenceCriterium);
            section.SummarizeSetting(SettingsItemType.McrCalculationRatioCutOff, _configuration.McrCalculationRatioCutOff);
            section.SummarizeSetting(SettingsItemType.McrCalculationTotalExposureCutOff, _configuration.McrCalculationTotalExposureCutOff);
            section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, _configuration.TargetDoseLevelType);
            section.SummarizeSetting(SettingsItemType.ClusterMethodType, _configuration.ClusterMethodType);
            if (_configuration.ClusterMethodType != ClusterMethodType.NoClustering) {
                section.SummarizeSetting(SettingsItemType.NumberOfClusters, _configuration.NumberOfClusters);
                if (_configuration.ClusterMethodType == ClusterMethodType.Hierarchical) {
                    section.SummarizeSetting(SettingsItemType.AutomaticallyDeterminationOfClusters, _configuration.AutomaticallyDeterminationOfClusters);
                }
            }
            if (_configuration.TargetDoseLevelType == TargetLevelType.Internal) {
                section.SummarizeSetting(SettingsItemType.ExposureCalculationMethod, _configuration.ExposureCalculationMethod);
                if (_configuration.ExposureCalculationMethod == ExposureCalculationMethod.MonitoringConcentration) {
                    section.SummarizeSetting(SettingsItemType.CodesHumanMonitoringSamplingMethods, string.Join(",", _configuration.CodesHumanMonitoringSamplingMethods));
                }
            }
            section.SummarizeSetting(SettingsItemType.NetworkAnalysisType, _configuration.NetworkAnalysisType);
            if (_configuration.NetworkAnalysisType != NetworkAnalysisType.NoNetworkAnalysis) {
                section.SummarizeSetting(SettingsItemType.IsLogTransform, _configuration.IsLogTransform);
            }
            return section;
        }
    }
}
