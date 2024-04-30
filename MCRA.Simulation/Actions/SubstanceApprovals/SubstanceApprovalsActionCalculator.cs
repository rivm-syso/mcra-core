using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.SubstanceApprovals {

    [ActionType(ActionType.SubstanceApprovals)]
    public class SubstanceApprovalsActionCalculator : ActionCalculatorBase<ISubstanceApprovalsActionResult> {

        public SubstanceApprovalsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.SubstanceApprovals][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var substanceApprovals = subsetManager.AllSubstanceApprovals;
            var dict = new Dictionary<Compound, SubstanceApproval>();
            foreach (var sa in substanceApprovals) {
                dict.Add(sa.Substance, sa);
            }
            data.SubstanceApprovals = dict;
        }

        protected override void summarizeActionResult(ISubstanceApprovalsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new SubstanceApprovalsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
