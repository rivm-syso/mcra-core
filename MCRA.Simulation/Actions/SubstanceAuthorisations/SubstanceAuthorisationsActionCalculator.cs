using MCRA.Utils.ProgressReporting;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.SubstanceAuthorisations {

    [ActionType(ActionType.SubstanceAuthorisations)]
    public class SubstanceAuthorisationsActionCalculator : ActionCalculatorBase<ISubstanceAuthorisationsActionResult> {

        public SubstanceAuthorisationsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.SubstanceAuthorisations][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.SubstanceAuthorisations][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var substanceAuthorisations = subsetManager.AllSubstanceAuthorisations;
            var dict = new Dictionary<(Food, Compound), SubstanceAuthorisation>();
            foreach (var use in substanceAuthorisations) {
                dict.Add((use.Food, use.Substance), use);
            }
            data.SubstanceAuthorisations = dict;
        }

        protected override void summarizeActionResult(ISubstanceAuthorisationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new SubstanceAuthorisationsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
