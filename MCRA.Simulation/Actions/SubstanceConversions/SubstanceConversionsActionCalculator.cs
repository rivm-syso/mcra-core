using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.SubstanceConversions {

    [ActionType(ActionType.SubstanceConversions)]
    public class SubstanceConversionsActionCalculator : ActionCalculatorBase<ISubstanceConversionsActionResult> {

        public SubstanceConversionsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.SubstanceConversions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var activeSubstances = data.ActiveSubstances.ToHashSet();
            var relevantMeasuredSubstances = subsetManager.AllSubstanceConversions
                    .Where(r => activeSubstances.Contains(r.ActiveSubstance))
                    .Select(r => r.MeasuredSubstance)
                    .ToHashSet();

            data.SubstanceConversions = subsetManager.AllSubstanceConversions
                .Where(r => relevantMeasuredSubstances.Contains(r.MeasuredSubstance))
                .ToList();
        }

        protected override void summarizeActionResult(ISubstanceConversionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new SubstanceConversionsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
