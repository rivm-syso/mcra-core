using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.ExposureBiomarkerConversions {

    [ActionType(ActionType.ExposureBiomarkerConversions)]
    public class ExposureBiomarkerConversionsActionCalculator : ActionCalculatorBase<IExposureBiomarkerConversionsActionResult> {

        public ExposureBiomarkerConversionsActionCalculator(ProjectDto project) : base(project) {
            _actionDataLinkRequirements[ScopingType.ExposureBiomarkerConversions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void verify() {
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ExposureBiomarkerConversionsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.ExposureBiomarkerConversions = subsetManager.AllExposureBiomarkerConversions;
        }

        protected override void summarizeActionResult(IExposureBiomarkerConversionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ExposureBiomarkerConversionsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
