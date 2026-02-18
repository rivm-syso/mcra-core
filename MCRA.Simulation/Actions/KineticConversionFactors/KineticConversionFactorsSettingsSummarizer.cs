using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.KineticConversionFactors {

    public class KineticConversionFactorsSettingsSummarizer : ActionModuleSettingsSummarizer<KineticConversionFactorsModuleConfig> {

        public KineticConversionFactorsSettingsSummarizer(KineticConversionFactorsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            if (_configuration.IsCompute) {
                section.SummarizeSetting(SettingsItemType.ExposureRoutes, _configuration.ExposureRoutes);
                section.SummarizeSetting(SettingsItemType.InternalMatrices, _configuration.InternalMatrices);
                section.SummarizeSetting(SettingsItemType.ExposureRangeMinimum, _configuration.ExposureRangeMinimum);
                section.SummarizeSetting(SettingsItemType.ExposureRangeMaximum, _configuration.ExposureRangeMaximum);
                section.SummarizeSetting(SettingsItemType.NumberOfPbkModelSimulations, _configuration.NumberOfPbkModelSimulations);
                section.SummarizeSetting(SettingsItemType.ComputeBetweenInternalTargetConversionFactors, _configuration.ComputeBetweenInternalTargetConversionFactors);
                section.SummarizeSetting(SettingsItemType.UseParameterVariability, _configuration.UseParameterVariability);
                if (_configuration.PbkSimulationMethod == PbkSimulationMethod.Standard) {
                    section.SummarizeSetting(SettingsItemType.NumberOfDays, _configuration.NumberOfDays);
                    section.SummarizeSetting(SettingsItemType.NonStationaryPeriod, _configuration.NonStationaryPeriod);
                }
                section.SummarizeSetting(SettingsItemType.ExposureEventsGenerationMethod, _configuration.ExposureEventsGenerationMethod);
                section.SummarizeSetting(SettingsItemType.AllowFallbackSystemic, _configuration.AllowFallbackSystemic);
                section.SummarizeSetting(SettingsItemType.PbkSimulationMethod, _configuration.PbkSimulationMethod);
                if (_configuration.PbkSimulationMethod != PbkSimulationMethod.Standard) {
                    section.SummarizeSetting(SettingsItemType.BodyWeightCorrected, _configuration.BodyWeightCorrected);
                    section.SummarizeSetting(SettingsItemType.NonStationaryPeriodInYears, _configuration.NonStationaryPeriodInYears);
                    section.SummarizeSetting(SettingsItemType.NonStationaryPeriod, _configuration.NonStationaryPeriod);
                    if (_configuration.PbkSimulationMethod == PbkSimulationMethod.LifetimeToSpecifiedAge) {
                        section.SummarizeSetting(SettingsItemType.LifetimeYears, _configuration.LifetimeYears);
                    }
                }

                section.SummarizeSetting(SettingsItemType.PbkOutputResolutionTimeUnit, _configuration.PbkOutputResolutionTimeUnit);
                if (_configuration.PbkOutputResolutionTimeUnit != PbkModelOutputResolutionTimeUnit.ModelTimeUnit) {
                    section.SummarizeSetting(SettingsItemType.PbkOutputResolutionStepSize, _configuration.PbkOutputResolutionStepSize);
                }
            } else {
                section.SummarizeSetting(SettingsItemType.KCFSubgroupDependent, _configuration.KCFSubgroupDependent);
            }
            return section;
        }
    }
}
