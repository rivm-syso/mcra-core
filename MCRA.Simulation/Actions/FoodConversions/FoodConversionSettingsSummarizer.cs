using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.FoodConversions {

    public sealed class FoodConversionSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.FoodConversions;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            var cs = project.ConversionSettings;
            var ass = project.AssessmentSettings;
            section.SummarizeSetting(SettingsItemType.SubstanceIndependent, cs.SubstanceIndependent);
            if (cs.UseProcessing) {
                section.SummarizeSetting(SettingsItemType.UseProcessing, cs.UseProcessing);
            }
            section.SummarizeSetting(SettingsItemType.UseComposition, cs.UseComposition);
            if (ass.TotalDietStudy) {
                section.SummarizeSetting(SettingsItemType.UseTdsComposition, ass.TotalDietStudy);
            }
            section.SummarizeSetting(SettingsItemType.UseReadAcrossFoodTranslations, cs.UseReadAcrossFoodTranslations);
            section.SummarizeSetting(SettingsItemType.UseMarketShares, cs.UseMarketShares);
            if (cs.UseMarketShares) {
                section.SummarizeSetting(SettingsItemType.UseSubTypes, cs.UseSubTypes);
            }
            section.SummarizeSetting(SettingsItemType.UseSuperTypes, cs.UseSuperTypes);
            section.SummarizeSetting(SettingsItemType.UseDefaultProcessingFactor, cs.UseDefaultProcessingFactor);
            return section;
        }
    }
}
