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

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());

            section.SummarizeSetting(SettingsItemType.ExposureType, _configuration.ExposureType);

            // Target level and (internal matrix and kinetic conversion)
            section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, _configuration.TargetDoseLevelType);
            if (_configuration.TargetDoseLevelType == TargetLevelType.Internal) {
                section.SummarizeSetting(SettingsItemType.CodeCompartment, _configuration.CodeCompartment);
                section.SummarizeSetting(SettingsItemType.InternalModelType, _configuration.InternalModelType);
                section.SummarizeSetting(SettingsItemType.StandardisedNormalisedUrine, _configuration.StandardisedNormalisedUrine);
                if (_configuration.StandardisedNormalisedUrine) {
                    section.SummarizeSetting(SettingsItemType.SelectedExpressionType, _configuration.SelectedExpressionType);
                }
                section.SummarizeSetting(SettingsItemType.StandardisedBlood, _configuration.StandardisedBlood);
            }

            // Sources and routes of exposure
            section.SummarizeSetting(SettingsItemType.ExposureRoutes, _configuration.ExposureRoutes, _configuration.ExposureRoutes.Any());
            section.SummarizeSetting(SettingsItemType.ExposureSources, _configuration.ExposureSources, _configuration.ExposureSources.Any());

            // Reference population and matching
            section.SummarizeSetting(SettingsItemType.IndividualReferenceSet, _configuration.IndividualReferenceSet);
            if (_configuration.ExposureSources.Count > 1) {
                if (_configuration.ExposureSources.Contains(ExposureSource.OtherNonDiet)
                    && _configuration.IndividualReferenceSet != ExposureSource.OtherNonDiet
                ) {
                    section.SummarizeSetting(SettingsItemType.NonDietaryPopulationAlignmentMethod, _configuration.NonDietaryPopulationAlignmentMethod);
                }
                if (_configuration.ExposureSources.Contains(ExposureSource.Dust)
                    && _configuration.IndividualReferenceSet != ExposureSource.Dust
                ) {
                    section.SummarizeSetting(SettingsItemType.DustPopulationAlignmentMethod, _configuration.DustPopulationAlignmentMethod);
                }
            }

            if (_configuration.RequirePbkModels) {
                section.SummarizeSetting(SettingsItemType.UseParameterVariability, _configuration.UseParameterVariability);
                section.SummarizeSetting(SettingsItemType.NumberOfDays, _configuration.NumberOfDays);
                section.SummarizeSetting(SettingsItemType.NonStationaryPeriod, _configuration.NonStationaryPeriod);
                section.SummarizeSetting(SettingsItemType.ExposureEventsGenerationMethod, _configuration.ExposureEventsGenerationMethod);
                if (_configuration.ExposureEventsGenerationMethod == ExposureEventsGenerationMethod.RandomDailyEvents) {
                    if (_configuration.SpecifyEvents) {
                        section.SummarizeSetting(SettingsItemType.SelectedEvents, _configuration.SelectedEvents);
                    }
                    if (_configuration.ExposureRoutes.Contains(ExposureRoute.Oral)) {
                        section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryOral, _configuration.NumberOfDosesPerDayNonDietaryOral);
                    }
                    if (_configuration.ExposureRoutes.Contains(ExposureRoute.Dermal)) {
                        section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryDermal, _configuration.NumberOfDosesPerDayNonDietaryDermal);
                    }
                    if (_configuration.ExposureRoutes.Contains(ExposureRoute.Inhalation)) {
                        section.SummarizeSetting(SettingsItemType.NumberOfDosesPerDayNonDietaryInhalation, _configuration.NumberOfDosesPerDayNonDietaryInhalation);
                    }
                }
            }

            // MCR analysis
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

