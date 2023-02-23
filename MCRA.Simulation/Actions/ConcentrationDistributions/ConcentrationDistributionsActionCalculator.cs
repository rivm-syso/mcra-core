using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.ConcentrationDistributions {

    [ActionType(ActionType.ConcentrationDistributions)]
    public class ConcentrationDistributionsActionCalculator : ActionCalculatorBase<IConcentrationDistributionsActionResult> {

        public ConcentrationDistributionsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            // Data linking requirements
            _actionDataLinkRequirements[ScopingType.ConcentrationDistributions][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ConcentrationDistributions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ConcentrationDistributionsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            // Load concentration distributions
            data.ConcentrationDistributions = new Dictionary<(Food, Compound), ConcentrationDistribution>();
            if (subsetManager.AllConcentrationDistributions?.Any() ?? false) {
                foreach (var record in subsetManager.AllConcentrationDistributions) {
                    data.ConcentrationDistributions.Add((record.Food, record.Compound), record);
                }
            }
        }

        protected override void summarizeActionResult(IConcentrationDistributionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ConcentrationDistributionsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
