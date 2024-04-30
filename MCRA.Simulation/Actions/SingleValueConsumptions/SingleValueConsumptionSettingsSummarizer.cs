using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.SingleValueConsumptions {

    public sealed class SingleValueConsumptionSettingsSummarizer : ActionModuleSettingsSummarizer<SingleValueConsumptionsModuleConfig> {

        public SingleValueConsumptionSettingsSummarizer(SingleValueConsumptionsModuleConfig config) : base(config) {
        }

        public override ActionSettingsSummary Summarize(bool isCompute, ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataOrCompute(isCompute, section);
            if (isCompute) {
                section.SummarizeSetting(SettingsItemType.ExposureType, _configuration.ExposureType);
                section.SummarizeSetting(SettingsItemType.IsDefaultSamplingWeight, _configuration.IsDefaultSamplingWeight);
                section.SummarizeSetting(SettingsItemType.IsProcessing, _configuration.IsProcessing);
                section.SummarizeSetting(SettingsItemType.UseBodyWeightStandardisedConsumptionDistribution, _configuration.UseBodyWeightStandardisedConsumptionDistribution);
                section.SummarizeSetting(SettingsItemType.ModelledFoodsConsumerDaysOnly, _configuration.ModelledFoodsConsumerDaysOnly);
            }
            return section;
        }
    }
}
