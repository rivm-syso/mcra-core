using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.HazardCharacterisations {

    public class HazardCharacterisationsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.HazardCharacterisations;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var es = project.EffectSettings;
            section.SummarizeSetting(SettingsItemType.ExposureType, project.AssessmentSettings.ExposureType);
            section.SummarizeSetting(SettingsItemType.Aggregate, project.AssessmentSettings.Aggregate);
            section.SummarizeSetting(SettingsItemType.TargetDoseLevelType, es.TargetDoseLevelType);
            section.SummarizeSetting(SettingsItemType.PointOfDeparture, es.PointOfDeparture);
            summarizeDataOrCompute(project, section);
            if (!project.CalculationActionTypes.Contains(ActionType)) {
                section.SummarizeSetting(SettingsItemType.RestrictToCriticalEffect, es.RestrictToCriticalEffect);
            }
            if (project.CalculationActionTypes.Contains(ActionType)) {
                section.SummarizeSetting(SettingsItemType.TargetDosesCalculationMethod, project.EffectSettings.TargetDosesCalculationMethod);
                
                section.SummarizeSetting(SettingsItemType.TargetDoseSelectionMethod, project.EffectSettings.TargetDoseSelectionMethod);
                section.SummarizeSetting(SettingsItemType.ImputeMissingHazardDoses, es.ImputeMissingHazardDoses);
                if (es.ImputeMissingHazardDoses) {
                    section.SummarizeSetting(SettingsItemType.HazardDoseImputationMethod, es.HazardDoseImputationMethod);
                }
                section.SummarizeSetting(SettingsItemType.UseDoseResponseModels, es.UseDoseResponseModels);
                section.SummarizeSetting(SettingsItemType.UseAdditionalAssessmentFactor, es.UseAdditionalAssessmentFactor);
                if (es.UseAdditionalAssessmentFactor) {
                    section.SummarizeSetting(SettingsItemType.AdditionalAssessmentFactor, es.AdditionalAssessmentFactor);
                }
                section.SummarizeSetting(SettingsItemType.UseInterSpeciesConversionFactors, es.UseInterSpeciesConversionFactors);
                section.SummarizeSetting(SettingsItemType.UseIntraSpeciesConversionFactors, es.UseIntraSpeciesConversionFactors);

            }
            return section;
        }
    }
}
