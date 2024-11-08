using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.ConsumptionsByModelledFood {
    public enum ConsumptionsByModelledFoodSections {
        ConsumptionPopulationSection,
        ConsumptionStatisticsSection,
        ConsumptionsFoodsAsEatenSection,
        ConsumptionsModelledFoodsSection,
        ConsumptionsProcessedModelledFoodsSection,
        MarketSharesSection

    }
    public sealed class ConsumptionsByModelledFoodSummarizer : ActionModuleResultsSummarizer<ConsumptionsByModelledFoodModuleConfig, ConsumptionsByModelledFoodActionResult> {

        private CompositeProgressState _progressState;

        public ConsumptionsByModelledFoodSummarizer(
            ConsumptionsByModelledFoodModuleConfig config,
            CompositeProgressState progressState = null
        ): base(config) {
            _progressState = progressState;
        }

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            ConsumptionsByModelledFoodActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<ConsumptionsByModelledFoodSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var consumptionInputSection = new ConsumptionsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var sub1Header = header.AddSubSectionHeaderFor(consumptionInputSection, ActionType.GetDisplayName(), order++);
            sub1Header.Units = collectUnits(data);
            sub1Header.SaveSummarySection(consumptionInputSection);

            if (result.ConsumptionsFoodsAsEaten != null && outputSettings.ShouldSummarize(ConsumptionsByModelledFoodSections.ConsumptionStatisticsSection)) {
                summarizeStatistics(data, result, sub1Header, order++);
                summarizeConsumptionStatistics(data, sub1Header, order++);
            }

            if (result.ConsumptionsFoodsAsEaten != null && outputSettings.ShouldSummarize(ConsumptionsByModelledFoodSections.ConsumptionsFoodsAsEatenSection)) {
                summarizeFoodsAsEaten(data, result, sub1Header, order++);
            }

            if (result.ConsumptionsByModelledFood != null && outputSettings.ShouldSummarize(ConsumptionsByModelledFoodSections.ConsumptionsModelledFoodsSection)) {
                summarizeModelledFoods(data, result, sub1Header, order++);
            }

            if (result.ConsumptionsByModelledFood != null
                && (data.ProcessingTypes?.Count > 0)
                && outputSettings.ShouldSummarize(ConsumptionsByModelledFoodSections.ConsumptionsProcessedModelledFoodsSection)) {
                summarizeProcessedModelledFoods(data, result, sub1Header, order++);
            }

            if (result.ConsumptionsByModelledFood.Any(c => c.IsBrand) && outputSettings.ShouldSummarize(ConsumptionsByModelledFoodSections.MarketSharesSection)) {
                summarizeMarketShares(result, sub1Header, order++);
            }
        }

        private List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new ActionSummaryUnitRecord("ConsumptionUnit", data.ConsumptionUnit.GetShortDisplayName()),
                new ActionSummaryUnitRecord("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"),
                new ActionSummaryUnitRecord("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}")
            };
            return result;
        }

        /// <summary>
        /// Consumption statistics foods as eaten/modelled foods
        /// </summary>
        /// <param name="data"></param>
        /// <param name="result"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeStatistics(ActionData data, ConsumptionsByModelledFoodActionResult result, SectionHeader header, int order) {
            var section = new IndividualConsumptionDataSection() {
                SectionLabel = getSectionLabel(ConsumptionsByModelledFoodSections.ConsumptionPopulationSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Survey individuals",
                order
            );
            section.Summarize(
                data.FoodSurvey,
                data.ModelledFoodConsumers,
                data.ModelledFoodConsumerDays,
                result.ConsumptionsFoodsAsEaten,
                result.ConsumptionsByModelledFood,
                IndividualSubsetType.IgnorePopulationDefinition,
                false,
                null
            );
            subHeader.SaveSummarySection(section);
        }


        private void summarizeConsumptionStatistics(ActionData data, SectionHeader header, int order) {
            var section = new ConsumptionDataSection() {
                SectionLabel = getSectionLabel(ConsumptionsByModelledFoodSections.ConsumptionStatisticsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Consumption statistics", order++);
            section.Summarize(
                data.ConsumerIndividualDays,
                data.SelectedFoodConsumptions,
                data.ConsumptionsByModelledFood
            );
            subHeader.SaveSummarySection(section);
        }
        /// <summary>
        /// Consumptions foods as eaten
        /// </summary>
        /// <param name="data"></param>
        /// <param name="result"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeFoodsAsEaten(ActionData data, ConsumptionsByModelledFoodActionResult result, SectionHeader header, int order) {
            var section = new FoodAsEatenConsumptionDataSection() {
                SectionLabel = getSectionLabel(ConsumptionsByModelledFoodSections.ConsumptionsFoodsAsEatenSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Consumptions foods as eaten",
                order
            );
            section.Summarize(
                data.ModelledFoodConsumerDays,
                data.AllFoods,
                result.ConsumptionsFoodsAsEaten,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage
            );
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Consumptions modelled foods
        /// </summary>
        /// <param name="project"></param>
        /// <param name="data"></param>
        /// <param name="result"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeModelledFoods(ActionData data, ConsumptionsByModelledFoodActionResult result, SectionHeader header, int order) {
            var section = new ModelledFoodConsumptionDataSection() {
                SectionLabel = getSectionLabel(ConsumptionsByModelledFoodSections.ConsumptionsModelledFoodsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Consumptions modelled foods",
                order
            );
            section.Summarize(
                data.ModelledFoodConsumerDays,
                data.AllFoods,
                data.ModelledFoods,
                result.ConsumptionsByModelledFood,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage
            );
            subHeader.SaveSummarySection(section);
        }

        /// <summary>
        /// Consumptions by processed modelled foods
        /// </summary>
        /// <param name="project"></param>
        /// <param name="data"></param>
        /// <param name="result"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeProcessedModelledFoods(ActionData data, ConsumptionsByModelledFoodActionResult result, SectionHeader header, int order) {
            var section = new ProcessedModelledFoodConsumptionSummarySection() {
                SectionLabel = getSectionLabel(ConsumptionsByModelledFoodSections.ConsumptionsProcessedModelledFoodsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Consumptions by processed modelled foods",
                order
            );
            section.Summarize(
                data.ModelledFoodConsumerDays,
                result.ConsumptionsByModelledFood,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage
            );
            subHeader.SaveSummarySection(section);
        }
        /// <summary>
        /// Market shares
        /// </summary>
        /// <param name="result"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        private void summarizeMarketShares(ConsumptionsByModelledFoodActionResult result, SectionHeader header, int order) {
            var section = new ModelledFoodMarketShareDataSection() {
                SectionLabel = getSectionLabel(ConsumptionsByModelledFoodSections.MarketSharesSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Market shares",
                order
            );
            section.Summarize(result.ConsumptionsByModelledFood);
            subHeader.SaveSummarySection(section);
        }
    }
}
