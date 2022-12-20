using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.FoodRecipes {
    public enum FoodRecipesSections {
        //No sub-sections
    }
    public sealed class FoodRecipesSummarizer : ActionResultsSummarizerBase<IFoodRecipesActionResult> {
        public override ActionType ActionType => ActionType.FoodRecipes;

        public override void Summarize(ProjectDto project, IFoodRecipesActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<FoodRecipesSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new FoodRecipesSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                ActionType.FoodRecipes.GetDisplayName(),
                order
            );
            section.Summarize(
                data.FoodRecipes,
                data.AllFoods,
                data.ProcessingTypes
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
