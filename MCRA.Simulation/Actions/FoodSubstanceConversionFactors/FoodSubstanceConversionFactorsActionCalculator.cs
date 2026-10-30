using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.FoodSubstanceConversionFactors {

    [ActionType(ActionType.FoodSubstanceConversionFactors)]
    public class FoodSubstanceConversionFactorsActionCalculator : ActionCalculatorBase<IFoodSubstanceConversionFactorsActionResult> {

        public FoodSubstanceConversionFactorsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.FoodSubstanceConversionFactors][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.FoodSubstanceConversionFactors][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }


        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            data.FoodSubstanceConversionFactors = subsetManager.AllFoodSubstanceConversionFactors;
            localProgress.Update(100);
        }

        protected override void summarizeActionResult(IFoodSubstanceConversionFactorsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new FoodSubstanceConversionFactorsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
