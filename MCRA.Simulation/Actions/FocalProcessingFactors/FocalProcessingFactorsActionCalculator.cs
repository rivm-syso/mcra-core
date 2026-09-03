using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.FocalProcessingFactors {

    [ActionType(ActionType.FocalProcessingFactors)]
    public class FocalProcessingFactorsActionCalculator : ActionCalculatorBase<IFocalProcessingFactorsActionResult> {

        public FocalProcessingFactorsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.FocalProcessingFactors][ScopingType.ProcessingTypes].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.FocalProcessingFactors][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.FocalProcessingFactors][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }


        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            data.FocalProcessingFactors = subsetManager.AllFocalProcessingFactors;

            localProgress.Update(100);
        }

        protected override void summarizeActionResult(IFocalProcessingFactorsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new FocalProcessingFactorsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
