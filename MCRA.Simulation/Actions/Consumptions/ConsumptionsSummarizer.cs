using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.Consumptions {
    public enum ConsumptionsSections {
        PopulationStatisticsSection,
        ConsumptionStatisticsSection,
        ConsumedFoodsSection
    }
    public sealed class ConsumptionsSummarizer : ActionResultsSummarizerBase<IConsumptionsActionResult> {

        public override ActionType ActionType => ActionType.Consumptions;

        public override void Summarize(ProjectDto project, IConsumptionsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConsumptionsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new ConsumptionsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(project, data);
            subHeader.SaveSummarySection(section);

            summarizePopulationStatistics(project, data, subHeader, order++);

            summarizeConsumptionStatistics(data, subHeader, order++);

            if (outputSettings.ShouldSummarize(ConsumptionsSections.ConsumedFoodsSection)) {
                summarizeConsumedFoods(project, data, subHeader, order++);
            }
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new ActionSummaryUnitRecord("ConsumptionUnit", data.ConsumptionUnit.GetShortDisplayName()),
                new ActionSummaryUnitRecord("LowerPercentage", $"p{project.OutputDetailSettings.LowerPercentage}"),
                new ActionSummaryUnitRecord("UpperPercentage", $"p{project.OutputDetailSettings.UpperPercentage}")
            };
            return result;
        }

        private void summarizePopulationStatistics(ProjectDto project, ActionData data, SectionHeader header, int order) {
            var section = new IndividualConsumptionDataSection() {
                SectionLabel = getSectionLabel(ConsumptionsSections.PopulationStatisticsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Survey individuals", order++);

            section.Summarize(
                data.FoodSurvey,
                data.ConsumerIndividuals,
                data.ConsumerIndividualDays,
                data.SelectedFoodConsumptions,
                data.ConsumptionsByModelledFood,
                project.SubsetSettings.MatchIndividualSubsetWithPopulation,
                project.SubsetSettings.PopulationSubsetSelection,
                data.SelectedPopulation
             
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeConsumptionStatistics(ActionData data, SectionHeader header, int order) {
            var section = new ConsumptionDataSection() {
                SectionLabel = getSectionLabel(ConsumptionsSections.ConsumptionStatisticsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Consumption statistics", order++);
            section.Summarize(
                data.ConsumerIndividualDays,
                data.SelectedFoodConsumptions,
                data.ConsumptionsByModelledFood
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeConsumedFoods(ProjectDto project, ActionData data, SectionHeader header, int order) {
            var section = new FoodAsEatenConsumptionDataSection() {
                SectionLabel = getSectionLabel(ConsumptionsSections.ConsumedFoodsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Consumed foods",
                order++
            );
            section.Summarize(
                data.ConsumerIndividualDays,
                data.AllFoods,
                data.SelectedFoodConsumptions,
                project.OutputDetailSettings.LowerPercentage,
                project.OutputDetailSettings.UpperPercentage
                );
            subHeader.SaveSummarySection(section);
        }
    }
}
