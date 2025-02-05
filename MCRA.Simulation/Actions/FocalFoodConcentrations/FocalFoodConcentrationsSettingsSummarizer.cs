using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.FocalFoodConcentrations {

    public sealed class FocalFoodConcentrationsSettingsSummarizer : ActionModuleSettingsSummarizer<FocalFoodConcentrationsModuleConfig> {

        public FocalFoodConcentrationsSettingsSummarizer(FocalFoodConcentrationsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(ProjectDto project = null) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            section.SummarizeSetting(SettingsItemType.FocalFoods, string.Join(", ", _configuration.FocalFoods.Select(r => r.CodeFood)));
            section.SummarizeSetting(SettingsItemType.FocalSubstances, string.Join(", ", _configuration.FocalFoods.Select(r => r.CodeSubstance)));

            section.SummarizeSetting(SettingsItemType.SampleSubsetSelection, _configuration.SampleSubsetSelection);
            if (_configuration.SampleSubsetSelection) {
                if (_configuration.SamplesSubsetDefinitions?.Count > 0) {
                    foreach (var subset in _configuration.SamplesSubsetDefinitions) {
                        if (subset.AlignSubsetWithPopulation) {
                            section.SummarizeSetting($"Sample subset on {subset.PropertyName}", "Aligned with population");
                        } else {
                            section.SummarizeSetting($"Sample subset on {subset.PropertyName}", string.Join(", ", subset.KeyWords.Distinct()));
                        }
                        section.SummarizeSetting($"Include records with unspecified {subset.PropertyName}", subset.IncludeMissingValueRecords);
                    }
                }
            }

            return section;
        }
    }
}
