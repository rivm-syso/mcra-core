using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.FoodRecipes {

    [ActionType(ActionType.FoodRecipes)]
    public class FoodRecipesActionCalculator : ActionCalculatorBase<IFoodRecipesActionResult> {

        public FoodRecipesActionCalculator(ProjectDto project) : base(project) {
            _actionDataLinkRequirements[ScopingType.FoodTranslations][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void verify() {
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new FoodRecipesSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.FoodRecipes = subsetManager.AllFoodTranslations
                .GroupBy(r => (r.FoodFrom, r.FoodTo))
                .Select(g => g.MaxBy(r => r.Proportion))
                .ToList();
        }

        protected override void summarizeActionResult(IFoodRecipesActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new FoodRecipesSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
