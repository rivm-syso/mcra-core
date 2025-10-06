using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Actions.ActiveSubstances;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ModelledFoods {

    public enum ModelledFoodsSections {
        ModelledFoodsSection,
        ModelledFoodSubstancesSection,
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

            if (outputSettings.ShouldSummarize(ModelledFoodsSections.ModelledFoodsSection)) {
                summarizeModelledFoods(result, subHeader, subOrder++);
            }

            if (outputSettings.ShouldSummarize(ModelledFoodsSections.ModelledFoodSubstancesSection)) {
                summarizeModelledFoodSubstances(result, subHeader, subOrder++);
            }
        }

        private void summarizeModelledFoods(ModelledFoodsActionResult result, SectionHeader header, int order) {
            var section = new ModelledFoodsSummarySection() {
                SectionLabel = getSectionLabel(ModelledFoodsSections.ModelledFoodsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Modelled foods", order);
            section.Summarize(result.ModelledFoodsInfos);
            subHeader.SaveSummarySection(section);
        }

        private void summarizeModelledFoodSubstances(ModelledFoodsActionResult result, SectionHeader header, int order) {
            var section = new ModelledFoodSubstancesSummarySection() {
                SectionLabel = getSectionLabel(ModelledFoodsSections.ModelledFoodSubstancesSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Modelled food/substance combinations", order);
            section.Summarize(result.ModelledFoodsInfos);
            subHeader.SaveSummarySection(section);
        }
    }
}
