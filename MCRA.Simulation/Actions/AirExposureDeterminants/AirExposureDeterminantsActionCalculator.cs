using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.AirExposureDeterminants {

    [ActionType(ActionType.AirExposureDeterminants)]
    public class AirExposureDeterminantsActionCalculator : ActionCalculatorBase<IAirExposureDeterminantsActionResult> {

        public AirExposureDeterminantsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
        }
        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.AirIndoorFractions = [.. subsetManager.AllAirIndoorFractions];
            data.AirVentilatoryFlowRates = [.. subsetManager.AllAirVentilatoryFlowRates];
            data.AirBodyExposureFractions = [.. subsetManager.AllAirBodyExposureFractions];
        }

        protected override void summarizeActionResult(IAirExposureDeterminantsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing air exposure determinants", 0);
            var summarizer = new AirExposureDeterminantsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
