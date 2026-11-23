using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.ConsumerProductUseFrequencies {

    [ActionType(ActionType.ConsumerProductUseFrequencies)]
    public class ConsumerProductUseFrequenciesActionCalculator(ProjectDto project) : ActionCalculatorBase<IConsumerProductUseFrequenciesActionResult>(project) {

        private ConsumerProductUseFrequenciesModuleConfig ModuleConfig => (ConsumerProductUseFrequenciesModuleConfig)_moduleSettings;

        protected override void verify() {
            _actionDataSelectionRequirements[ScopingType.ConsumerProductUseFrequencies].AllowCodesInScopeNotInSource = true;
            _actionDataSelectionRequirements[ScopingType.ConsumerProductIndividualProperties].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.ConsumerProductIndividualPropertyValues].AllowEmptyScope = true;
            _actionDataLinkRequirements[ScopingType.ConsumerProductSurveys][ScopingType.Populations].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ConsumerProductIndividualPropertyValues][ScopingType.ConsumerProductIndividuals].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ConsumerProductUseFrequencies][ScopingType.ConsumerProducts].AlertTypeMissingData = AlertType.Notification;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ConsumerProductUseFrequenciesSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.ConsumerProductSurveys = [.. subsetManager.AllConsumerProductSurveys.Values];

            var allConsumerProductsIndividualProperties = subsetManager.AllConsumerProductIndividualProperties;
            var individuals = IndividualsSubsetCalculator.GetIndividualSubsets(
                subsetManager.AllConsumerProductIndividuals,
                subsetManager.AllConsumerProductIndividualProperties,
                data.SelectedPopulation,
                [.. data.ConsumerProductSurveys.Select(c => c.Code)],
                ModuleConfig.MatchCPIndividualSubsetWithPopulation,
                ModuleConfig.SelectedCPSurveySubsetProperties,
                true
            );

            data.ConsumerProductIndividuals = individuals;
            var useFrequencies = subsetManager.AllIndividualConsumerProductUseFrequencies;
            // Load consumer product use frequencies and check that each survey contains unique products
            var groups = useFrequencies.GroupBy(c => c.Individual.CodeFoodSurvey).ToList();
            var cps = new HashSet<ConsumerProduct>();
            foreach (var group in groups) {
                var products = group.Select(c => c.Product).ToHashSet();
                if (cps.Overlaps(products)) {
                    throw new Exception($"Consumer product use frequencies for survey {group.Key} contain products that are already present in other surveys. " +
                        $"Each survey must contain unique products.");
                } else {
                    cps.UnionWith(products);
                }
            }
            data.AllIndividualConsumerProductUseFrequencies = useFrequencies;
        }

        protected override void summarizeActionResult(IConsumerProductUseFrequenciesActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            var summarizer = new ConsumerProductUseFrequenciesSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
