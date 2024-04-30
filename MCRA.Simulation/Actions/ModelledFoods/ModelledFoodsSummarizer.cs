using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.ModelledFoods {
    public enum ModelledFoodsSections {
        ModelledFoodsSection,
    }
    public sealed class ModelledFoodsSummarizer : ActionResultsSummarizerBase<ModelledFoodsActionResult> {
        public override ActionType ActionType => ActionType.ModelledFoods;

        public override void Summarize(ActionModuleConfig sectionConfig, ModelledFoodsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ModelledFoodsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;
            summarizeModelledFoods(result, subHeader, subOrder++);
        }

        private void summarizeModelledFoods(ModelledFoodsActionResult result, SectionHeader header, int order) {
            var section = new ModelledFoodsSummarySection() {
                SectionLabel = getSectionLabel(ModelledFoodsSections.ModelledFoodsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(result.ModelledFoodsInfos);
            subHeader.SaveSummarySection(section);
        }
    }
}
