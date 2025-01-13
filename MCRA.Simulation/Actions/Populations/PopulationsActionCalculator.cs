using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.PopulationDefinitionCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.Populations {

    [ActionType(ActionType.Populations)]
    public class PopulationsActionCalculator : ActionCalculatorBase<IPopulationsActionResult> {
        private PopulationsModuleConfig ModuleConfig => (PopulationsModuleConfig)_moduleSettings;

        public PopulationsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            if (!_moduleSettings.IsCompute) {
                _actionDataSelectionRequirements[ScopingType.Populations].MaxSelectionCount = 1;
            }
            _actionDataSelectionRequirements[ScopingType.PopulationIndividualProperties].AllowEmptyScope = true;
            _actionDataLinkRequirements[ScopingType.PopulationIndividualPropertyValues][ScopingType.Populations].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.PopulationIndividualPropertyValues][ScopingType.PopulationIndividualProperties].AlertTypeMissingData = AlertType.Notification;
        }

        public override bool CheckDataDependentSettings(ICompiledLinkManager linkManager) {
            if (_moduleSettings.IsCompute) {
                return true;
            }
            return linkManager.GetCodesInScope(ScopingType.Populations).Count == 1;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new PopulationsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            if (!subsetManager.AllPopulations.Any()) {
                throw new Exception("No population found.");
            } else if (subsetManager.AllPopulations.Count > 1) {
                throw new Exception("Multiple populations selected.");
            }
            data.SelectedPopulation = subsetManager.AllPopulations.FirstOrDefault();
            // If we want to allow further specification of the selected population with
            // additional subset specifications, then we could do something like this.
            //if (Recude selected population with additional subset specifications) {
            //    data.SelectedPopulation.PopulationIndividualPropertyValues = PopulationIndividualPropertyCalculator.CreatePopulationIndividualPropertyValues(
            //        ModuleConfig.IndividualsSubsetDefinitions,
            //        ModuleConfig.IndividualDaySubsetDefinition
            //    );
            //}
        }

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
        /// Summarizes the action result.
        /// </summary>
        /// <param name="actionResult"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="order"></param>
        /// <param name="progressReport"></param>
        protected override void summarizeActionResult(IPopulationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(60);
            localProgress.Update("Summarizing Population", 0);
            var summarizer = new PopulationsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}