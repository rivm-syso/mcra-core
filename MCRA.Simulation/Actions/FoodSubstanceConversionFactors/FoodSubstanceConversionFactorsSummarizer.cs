using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.FoodSubstanceConversionFactors {

    public enum FoodSubstanceConversionFactorsSections {
        FoodSubstanceConversionFactorsSection
    }

    public sealed class FoodSubstanceConversionFactorsSummarizer : ActionResultsSummarizerBase<IFoodSubstanceConversionFactorsActionResult> {

        public override ActionType ActionType => ActionType.FoodSubstanceConversionFactors;

        public override void Summarize(
            ActionModuleConfig outputConfig,
            IFoodSubstanceConversionFactorsActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<FoodSubstanceConversionFactorsSections>(outputConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;
            if (data.FoodSubstanceConversionFactors != null && outputSettings.ShouldSummarize(FoodSubstanceConversionFactorsSections.FoodSubstanceConversionFactorsSection)) {
                summarizeFoodSubstanceConversionFactors(data, subHeader, subOrder++);
            }
        }

        private void summarizeFoodSubstanceConversionFactors(ActionData data, SectionHeader header, int order) {
            var section = new FoodSubstanceConversionFactorsSummarySection() {
                SectionLabel = getSectionLabel(FoodSubstanceConversionFactorsSections.FoodSubstanceConversionFactorsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Food specific conversion factors",
                order++
            );
            section.Summarize(data.FoodSubstanceConversionFactors);
            subHeader.SaveSummarySection(section);
        }
    }
}
