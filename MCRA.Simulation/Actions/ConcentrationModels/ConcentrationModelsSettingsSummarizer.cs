using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.SettingsDefinitions;
using MCRA.Simulation.Action;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConcentrationModels {

    public sealed class ConcentrationModelsSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.ConcentrationModels;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());

            var cms = project.ConcentrationModelSettings;
            section.SummarizeSetting(SettingsItemType.ConcentrationModelChoice, cms.ConcentrationModelChoice);
            section.SummarizeSetting(SettingsItemType.DefaultConcentrationModel, cms.DefaultConcentrationModel);
            section.SummarizeSetting(SettingsItemType.IsFallbackMrl, cms.IsFallbackMrl);
            if (cms.IsFallbackMrl) {
                section.SummarizeSetting(SettingsItemType.FractionOfMrl, cms.FractionOfMrl);
            }
            section.SummarizeSetting(SettingsItemType.RestrictLorImputationToAuthorisedUses, cms.RestrictLorImputationToAuthorisedUses);
            section.SummarizeSetting(SettingsItemType.NonDetectsHandlingMethod, cms.NonDetectsHandlingMethod);
            if (cms.NonDetectsHandlingMethod != NonDetectsHandlingMethod.ReplaceByZero) {
                section.SummarizeSetting(SettingsItemType.FractionOfLOR, cms.FractionOfLOR);
            }
            section.SummarizeSetting(SettingsItemType.IsSampleBased, cms.IsSampleBased);
            if (cms.IsSampleBased) {
                section.SummarizeSetting(SettingsItemType.ImputeMissingValues, cms.ImputeMissingValues);
                if (cms.ImputeMissingValues) {
                    section.SummarizeSetting(SettingsItemType.CorrelateImputedValueWithSamplePotency, cms.CorrelateImputedValueWithSamplePotency);
                }
            }
            if (project.AssessmentSettings.TotalDietStudy) {
                section.SummarizeSetting(SettingsItemType.TotalDietStudy, project.AssessmentSettings.TotalDietStudy);
            }
            section.SummarizeSetting(SettingsItemType.Cumulative, project.AssessmentSettings.Cumulative, isVisible: false);
            return section;
        }
    }
}
