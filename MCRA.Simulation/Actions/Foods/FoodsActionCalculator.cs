using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.Foods {

    [ActionType(ActionType.Foods)]
    public sealed class FoodsActionCalculator : ActionCalculatorBase<IFoodsActionResult> {

        public FoodsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataSelectionRequirements[ScopingType.Foods].AllowCodesInScopeNotInSource = true;
            _actionDataSelectionRequirements[ScopingType.ProcessingTypes].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.ProcessingTypes].AllowCodesInScopeNotInSource = true;
            _actionDataSelectionRequirements[ScopingType.Facets].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.Facets].AllowCodesInScopeNotInSource = true;
            _actionDataSelectionRequirements[ScopingType.FacetDescriptors].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.FacetDescriptors].AllowCodesInScopeNotInSource = true;
            _actionDataLinkRequirements[ScopingType.FoodHierarchies][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.FoodProperties][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.FoodConsumptionQuantifications][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.FoodOrigins][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.FoodUnitWeights][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new FoodsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.AllFoods = subsetManager.AllFoods;
            data.AllFoodsByCode = subsetManager.AllFoodsByCode;
            data.ProcessingTypes = subsetManager.AllProcessingTypes;
        }

        protected override void summarizeActionResult(IFoodsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            var summarizer = new FoodsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);

        }
    }
}
