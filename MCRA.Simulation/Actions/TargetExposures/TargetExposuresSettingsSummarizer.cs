using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.TargetExposures {

    public sealed class TargetExposuresSettingsSummarizer : ActionModuleSettingsSummarizer<TargetExposuresModuleConfig> {

        public TargetExposuresSettingsSummarizer(TargetExposuresModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.ExposureType, _configuration.ExposureType);
            section.SummarizeSetting(SettingsItemType.Aggregate, _configuration.Aggregate);
            if (_configuration.Aggregate) {
                section.SummarizeSetting(SettingsItemType.MatchSpecificIndividuals, _configuration.MatchSpecificIndividuals);
                if (!_configuration.MatchSpecificIndividuals) {
                    section.SummarizeSetting(SettingsItemType.IsCorrelationBetweenIndividuals, _configuration.IsCorrelationBetweenIndividuals);
                }
            }
            section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, _configuration.TargetDoseLevelType);
            if (_configuration.ExposureType == ExposureType.Chronic) {
                section.SummarizeSetting(SettingsItemType.IntakeModelType, _configuration.IntakeModelType);
            }
            if (_configuration.FirstModelThenAdd) {
                section.SummarizeSetting(SettingsItemType.FirstModelThenAdd, _configuration.FirstModelThenAdd);
            }
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

