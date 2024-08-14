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

            var isKineticConversionModel = _configuration.InternalModelType == InternalModelType.AbsorptionFactorModel;
            if (!isKineticConversionModel) {
                section.SummarizeSetting(SettingsItemType.CodeCompartment, _configuration.CodeCompartment);
            }

            if (_configuration.IntakeFirstModelThenAdd) {
                section.SummarizeSetting(SettingsItemType.IntakeFirstModelThenAdd, _configuration.IntakeFirstModelThenAdd);
            }
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

