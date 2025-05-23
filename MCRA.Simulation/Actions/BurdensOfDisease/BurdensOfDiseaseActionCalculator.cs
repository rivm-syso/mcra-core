using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.BurdensOfDisease {

    [ActionType(ActionType.BurdensOfDisease)]
    public class BurdensOfDiseaseActionCalculator : ActionCalculatorBase<IIBurdensOfDiseaseActionResult> {

        public BurdensOfDiseaseActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.BurdensOfDisease][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.BurdensOfDisease][ScopingType.Populations].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.BurdensOfDisease = subsetManager.AllBurdensOfDisease
                .Where(r => r.Population == null
                    || data.SelectedPopulation.Code == "Generated"
                    || r.Population == data.SelectedPopulation
                )
                .ToList();
        }

        protected override void summarizeActionResult(IIBurdensOfDiseaseActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new BurdensOfDiseaseSummarizer();
            summarizer.Summarize(_project.ActionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
