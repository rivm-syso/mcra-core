using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.ConsumerProductUseFrequencies {

    [ActionType(ActionType.ConsumerProductUseFrequencies)]
    public sealed class ConsumerProductUseFrequenciesActionCalculator : ActionCalculatorBase<IConsumerProductUseFrequenciesActionResult> {

        public ConsumerProductUseFrequenciesActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataSelectionRequirements[ScopingType.ConsumerProductUseFrequencies].AllowCodesInScopeNotInSource = true;
            _actionDataSelectionRequirements[ScopingType.ConsumerProductIndividualProperties].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.ConsumerProductIndividualPropertyValues].AllowEmptyScope = true;
            _actionDataLinkRequirements[ScopingType.ConsumerProductSurveys][ScopingType.Populations].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ConsumerProductIndividualPropertyValues][ScopingType.ConsumerProductIndividuals].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ConsumerProductUseFrequencies][ScopingType.ConsumerProducts].AlertTypeMissingData = AlertType.Notification;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ConsumerProductUseFrequenciesSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.ConsumerProductSurveys = [.. subsetManager.AllConsumerProductSurveys.Values];
            data.ConsumerProductIndividuals = subsetManager.AllConsumerProductIndividuals;
            data.AllIndividualConsumerProductUseFrequencies = subsetManager.AllIndividualConsumerProductUseFrequencies;
        }

        protected override void summarizeActionResult(IConsumerProductUseFrequenciesActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            var summarizer = new ConsumerProductUseFrequenciesSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
