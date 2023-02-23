using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.ConsumptionsByModelledFood {
    public enum ConsumptionsByModelledFoodSections {
        ConsumptionPopulationSection,
        ConsumptionStatisticsSection,
        ConsumptionsFoodsAsEatenSection,
        ConsumptionsModelledFoodsSection,
        ConsumptionsProcessedModelledFoodsSection,
        MarketSharesSection

    }
    public sealed class ConsumptionsByModelledFoodSummarizer : ActionResultsSummarizerBase<ConsumptionsByModelledFoodActionResult> {

        private CompositeProgressState _progressState;

        public override ActionType ActionType => ActionType.ConsumptionsByModelledFood;

        public ConsumptionsByModelledFoodSummarizer(CompositeProgressState progressState = null) {
            _progressState = progressState;
        }

        public override void Summarize(ProjectDto project, ConsumptionsByModelledFoodActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConsumptionsByModelledFoodSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var consumptionInputSection = new ConsumptionInputSection() {
                SectionLabel = ActionType.ToString()
            };
            var sub1Header = header.AddSubSectionHeaderFor(consumptionInputSection, ActionType.GetDisplayName(), order++);
            sub1Header.Units = collectUnits(project, data);
            sub1Header.SaveSummarySection(consumptionInputSection);

            if (result.ConsumptionsFoodsAsEaten != null && outputSettings.ShouldSummarize(ConsumptionsByModelledFoodSections.ConsumptionStatisticsSection)) {
                summarizeStatistics(data, result, sub1Header, order++);
                summarizeConsumptionStatistics(data, sub1Header, order++);
            }

            if (result.ConsumptionsFoodsAsEaten != null && outputSettings.ShouldSummarize(ConsumptionsByModelledFoodSections.ConsumptionsFoodsAsEatenSection)) {
                summarizeFoodsAsEaten(project, data, result, sub1Header, order++);
            }

            if (result.ConsumptionsByModelledFood != null && outputSettings.ShouldSummarize(ConsumptionsByModelledFoodSections.ConsumptionsModelledFoodsSection)) {
                summarizeModelledFoods(project, data, result, sub1Header, order++);
            }

            if (result.ConsumptionsByModelledFood != null
                && (data.ProcessingTypes?.Any() ?? false)
                && outputSettings.ShouldSummarize(ConsumptionsByModelledFoodSections.ConsumptionsProcessedModelledFoodsSection)) {
                summarizeProcessedModelledFoods(project, data, result, sub1Header, order++);
            }

            if (result.ConsumptionsByModelledFood.Any(c => c.IsBrand) && outputSettings.ShouldSummarize(ConsumptionsByModelledFoodSections.MarketSharesSection)) {
                summarizeMarketShares(result, sub1Header, order++);
            }
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord>();
            result.Add(new ActionSummaryUnitRecord("ConsumptionUnit", data.ConsumptionUnit.GetShortDisplayName()));
            result.Add(new ActionSummaryUnitRecord("LowerPercentage", $"p{project.OutputDetailSettings.LowerPercentage}"));
            result.Add(new ActionSummaryUnitRecord("UpperPercentage", $"p{project.OutputDetailSettings.UpperPercentage}"));
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
        private void summarizeFoodsAsEaten(ProjectDto project, ActionData data, ConsumptionsByModelledFoodActionResult result, SectionHeader header, int order) {
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
                project.OutputDetailSettings.LowerPercentage,
                project.OutputDetailSettings.UpperPercentage
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
        private void summarizeModelledFoods(ProjectDto project, ActionData data, ConsumptionsByModelledFoodActionResult result, SectionHeader header, int order) {
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
                project.OutputDetailSettings.LowerPercentage,
                project.OutputDetailSettings.UpperPercentage
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
        private void summarizeProcessedModelledFoods(ProjectDto project, ActionData data, ConsumptionsByModelledFoodActionResult result, SectionHeader header, int order) {
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
                project.OutputDetailSettings.LowerPercentage,
                project.OutputDetailSettings.UpperPercentage
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
