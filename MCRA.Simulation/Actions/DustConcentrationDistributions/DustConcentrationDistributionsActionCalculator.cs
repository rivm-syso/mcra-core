using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.DustConcentrationDistributions {

    [ActionType(ActionType.DustConcentrationDistributions)]
    public class DustConcentrationDistributionsActionCalculator : ActionCalculatorBase<IDustConcentrationDistributionsActionResult> {

        public DustConcentrationDistributionsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.DustConcentrationDistributions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.DustConcentrationDistributions = subsetManager.AllDustConcentrationDistributions.ToList();
        }

        protected override void summarizeActionResult(IDustConcentrationDistributionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing dust concentration distributions", 0);
            var summarizer = new DustConcentrationDistributionsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
