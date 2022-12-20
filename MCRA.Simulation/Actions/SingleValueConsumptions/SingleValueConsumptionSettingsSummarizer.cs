using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.SettingsDefinitions;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.SingleValueConsumptions {

    public sealed class SingleValueConsumptionSettingsSummarizer : ActionSettingsSummarizerBase {

        public override ActionType ActionType => ActionType.SingleValueConsumptions;

        public override ActionSettingsSummary Summarize(ProjectDto project) {
            var section = new ActionSettingsSummary(ActionType.GetDisplayName());
            summarizeDataOrCompute(project, section);
            if (project.CalculationActionTypes.Contains(ActionType)) {
                section.SummarizeSetting(SettingsItemType.ExposureType, project.AssessmentSettings.ExposureType);
                section.SummarizeSetting(SettingsItemType.IsDefaultSamplingWeight, project.SubsetSettings.IsDefaultSamplingWeight);
                section.SummarizeSetting(SettingsItemType.IsProcessing, project.ConcentrationModelSettings.IsProcessing);
                section.SummarizeSetting(SettingsItemType.UseBodyWeightStandardisedConsumptionDistribution, project.SubsetSettings.UseBodyWeightStandardisedConsumptionDistribution);
                section.SummarizeSetting(SettingsItemType.ModelledFoodsConsumerDaysOnly, project.SubsetSettings.ModelledFoodsConsumerDaysOnly);
            }
            return section;
        }
    }
}
