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
                if (_configuration.ExposureSources.Contains(ExposureSource.OtherNonDietary)
                    && _configuration.IndividualReferenceSet != ExposureSource.OtherNonDietary
                ) {
                    section.SummarizeSetting(SettingsItemType.NonDietaryPopulationAlignmentMethod, _configuration.NonDietaryPopulationAlignmentMethod);
                }
                if (_configuration.ExposureSources.Contains(ExposureSource.DustExposures)
                    && _configuration.IndividualReferenceSet != ExposureSource.DustExposures
                ) {
                    section.SummarizeSetting(SettingsItemType.DustPopulationAlignmentMethod, _configuration.DustPopulationAlignmentMethod);
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

