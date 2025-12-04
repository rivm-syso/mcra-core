using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.PopulationDefinitionCalculation;
using MCRA.Simulation.Objects.Populations;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.Populations {

    [ActionType(ActionType.Populations)]
    public class PopulationsActionCalculator : ActionCalculatorBase<IPopulationsActionResult> {
        private PopulationsModuleConfig ModuleConfig => (PopulationsModuleConfig)_moduleSettings;

        public PopulationsActionCalculator(ProjectDto project) : base(project) {
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResamplePopulationSizes) {
                result.Add(UncertaintySource.PopulationSizes);
            }
            return result;
        }

        protected override void verify() {
            if (!_moduleSettings.IsCompute) {
                _actionDataSelectionRequirements[ScopingType.Populations].MaxSelectionCount = 1;
            }
            _actionDataSelectionRequirements[ScopingType.PopulationIndividualProperties].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.PopulationCharacteristics].AllowEmptyScope = true;
            _actionDataLinkRequirements[ScopingType.PopulationIndividualPropertyValues][ScopingType.Populations].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.PopulationIndividualPropertyValues][ScopingType.PopulationIndividualProperties].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.PopulationCharacteristics][ScopingType.Populations].AlertTypeMissingData = AlertType.Notification;
        }

        public override bool CheckDataDependentSettings(ICompiledLinkManager linkManager) {
            return _moduleSettings.IsCompute || linkManager.GetCodesInScope(ScopingType.Populations).Count == 1;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new PopulationsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        /// <summary>
        /// Load data implementation.
        /// </summary>
        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            if (subsetManager.AllPopulations.Count == 0) {
                throw new Exception("No population found.");
            } else if (subsetManager.AllPopulations.Count > 1) {
                throw new Exception("Multiple populations selected.");
            }
            var population = subsetManager.AllPopulations.FirstOrDefault();
            data.SelectedPopulation = new SimulatedPopulation(population);
        }

        /// <summary>
        /// Compute population.
        /// </summary>
        protected override IPopulationsActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var result = new PopulationsActionResult();
            localProgress.Update(100);
            var populationBuilder = new PopulationDefinitionBuilder();
            data.SelectedPopulation = populationBuilder.Create(
                ModuleConfig.NominalPopulationBodyWeight,
                ModuleConfig.PopulationSubsetSelection,
                ModuleConfig.IndividualsSubsetDefinitions,
                ModuleConfig.IndividualDaySubsetDefinition
            );
            return result;
        }

        /// <summary>
        /// Load and resample distribution for population sizes.
        /// </summary>
        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            if (data.SelectedPopulation != null && factorialSet.Contains(UncertaintySource.PopulationSizes)) {
                localProgress.Update("Resampling population sizes.");
                var random = uncertaintySourceGenerators[UncertaintySource.PopulationSizes];
                (data.SelectedPopulation as SimulatedPopulation).ResamplePopulationSize(random);
            }
            localProgress.Update(100);
        }

        /// <summary>
        /// Summarizes the action result.
        /// </summary>
        protected override void summarizeActionResult(IPopulationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            localProgress.Update("Summarizing Population", 0);
            var summarizer = new PopulationsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
