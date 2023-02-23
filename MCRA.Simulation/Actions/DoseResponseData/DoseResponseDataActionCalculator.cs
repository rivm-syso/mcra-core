using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.DoseResponseDataCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.DoseResponseData {

    [ActionType(ActionType.DoseResponseData)]
    public class DoseResponseDataActionCalculator : ActionCalculatorBase<IDoseResponseDataActionResult> {
        public DoseResponseDataActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.DoseResponseExperiments][ScopingType.Responses].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.DoseResponseExperiments][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.DoseResponseExperimentDoses][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.DoseResponseExperimentMeasurements][ScopingType.Responses].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ExperimentalUnitProperties][ScopingType.DoseResponseExperiments].AlertTypeMissingData = AlertType.Notification;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new DoseResponseDataSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var availableDoseResponseExperiments = subsetManager.AllDoseResponseExperiments;
            data.AvailableDoseResponseExperiments = availableDoseResponseExperiments;
            if (_project.EffectSettings.MergeDoseResponseExperimentsData) {
                var doseResponseDataMerger = new DoseResponseDataMerger();
                var mergedExperiments = availableDoseResponseExperiments
                    .SelectMany(r => r.Responses, (e, r) => (e, r))
                    .GroupBy(r => r.r)
                    .Select(g => doseResponseDataMerger.Merge(g.Select(r => r.e).ToList(), g.Key))
                    .ToList();
                data.SelectedResponseExperiments = mergedExperiments;
            } else {
                data.SelectedResponseExperiments = availableDoseResponseExperiments;
            }
            if (!data.SelectedResponseExperiments.Any()) {
                throw new Exception("No dose response data found for the selected substance(s)");
            }
        }

        protected override void summarizeActionResult(IDoseResponseDataActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing dose response data", 0);
            var summarizer = new DoseResponseDataSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}