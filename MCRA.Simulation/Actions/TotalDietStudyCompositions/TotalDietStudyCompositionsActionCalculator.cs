using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.TotalDietStudyCompositions {

    [ActionType(ActionType.TotalDietStudyCompositions)]
    public class TotalDietStudyCompositionsActionCalculator : ActionCalculatorBase<ITotalDietStudyCompositionsActionResult> {

        public TotalDietStudyCompositionsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.TdsFoodSampleCompositions][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new TotalDietStudyCompositionsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.TdsFoodCompositions = subsetManager.AllTDSFoodSampleCompositions.ToLookup(c => c.TDSFood);
        }

        protected override void summarizeActionResult(ITotalDietStudyCompositionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing total diet studies", 0);
            var summarizer = new TotalDietStudyCompositionsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
