using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.FoodExtrapolations {
    public enum FoodExtrapolationsSections {
        //No sub-sections
    }
    public sealed class FoodExtrapolationsSummarizer : ActionResultsSummarizerBase<IFoodExtrapolationsActionResult> {
        public override ActionType ActionType => ActionType.FoodExtrapolations;

        public override void Summarize(ActionModuleConfig sectionConfig, IFoodExtrapolationsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<FoodExtrapolationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new FoodExtrapolationsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.FoodExtrapolations.GetDisplayName(), order);
            section.Summarize(data.FoodExtrapolations);
            subHeader.SaveSummarySection(section);
        }
    }
}
