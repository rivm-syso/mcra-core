using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.Consumptions {
    public enum ConsumptionsSections {
        PopulationStatisticsSection,
        ConsumptionStatisticsSection,
        ConsumedFoodsSection
    }
    public sealed class ConsumptionsSummarizer : ActionModuleResultsSummarizer<ConsumptionsModuleConfig, IConsumptionsActionResult> {

        public ConsumptionsSummarizer(ConsumptionsModuleConfig config) : base(config) {
        }

        public override void Summarize(ActionModuleConfig sectionConfig, IConsumptionsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConsumptionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            subHeader.Units = collectUnits(data);

            summarizePopulationStatistics(data, subHeader, order++);

            summarizeConsumptionStatistics(data, subHeader, order++);

            if (outputSettings.ShouldSummarize(ConsumptionsSections.ConsumedFoodsSection)) {
                summarizeConsumedFoods(data, subHeader, order++);
            }
        }

        private List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("ConsumptionUnit", data.ConsumptionUnit.GetShortDisplayName()),
                new("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}")
            };
            return result;
        }

        private void summarizePopulationStatistics(ActionData data, SectionHeader header, int order) {
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
                _configuration.MatchIndividualSubsetWithPopulation,
                _configuration.PopulationSubsetSelection,
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

        private void summarizeConsumedFoods(ActionData data, SectionHeader header, int order) {
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
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage
                );
            subHeader.SaveSummarySection(section);
        }
    }
}
