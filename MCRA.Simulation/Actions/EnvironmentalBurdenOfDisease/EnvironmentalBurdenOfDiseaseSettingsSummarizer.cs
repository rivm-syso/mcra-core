using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease {

    public sealed class EnvironmentalBurdenOfDiseaseSettingsSummarizer : ActionModuleSettingsSummarizer<EnvironmentalBurdenOfDiseaseModuleConfig> {
        public EnvironmentalBurdenOfDiseaseSettingsSummarizer(EnvironmentalBurdenOfDiseaseModuleConfig config) : base(config) {
        }

        public override ActionType ActionType => ActionType.EnvironmentalBurdenOfDisease;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());

            section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, _configuration.TargetDoseLevelType);
            section.SummarizeSetting(SettingsItemType.ExposureCalculationMethod, _configuration.ExposureCalculationMethod);
            section.SummarizeSetting(
                SettingsItemType.BodIndicators,
                string.Join(", ", _configuration.BodIndicators)
            );
            section.SummarizeSetting(SettingsItemType.BodApproach, _configuration.BodApproach);
            section.SummarizeSetting(SettingsItemType.ExposureGroupingMethod, _configuration.ExposureGroupingMethod);
            section.SummarizeSetting(SettingsItemType.BinBoundaries, _configuration.BinBoundaries);
            section.SummarizeSetting(SettingsItemType.EbdStandardisation, _configuration.EbdStandardisation);
            if (_configuration.ExposureCalculationMethod == ExposureCalculationMethod.MonitoringConcentration) {
                section.SummarizeSetting(SettingsItemType.UsePointEstimates, _configuration.UsePointEstimates);
            }
            section.SummarizeSetting(SettingsItemType.WithinBinExposureRepresentationMethod, _configuration.WithinBinExposureRepresentationMethod);

            return section;
        }
    }
}
