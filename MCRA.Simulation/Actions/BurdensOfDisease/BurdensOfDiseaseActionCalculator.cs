using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.BodIndicatorModels;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.BurdensOfDisease {

    [ActionType(ActionType.BurdensOfDisease)]
    public class BurdensOfDiseaseActionCalculator : ActionCalculatorBase<IBurdensOfDiseaseActionResult> {

        private BurdensOfDiseaseModuleConfig ModuleConfig => (BurdensOfDiseaseModuleConfig)_moduleSettings;

        public BurdensOfDiseaseActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.BurdensOfDisease][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.BurdensOfDisease][ScopingType.Populations].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleBodIndicatorValues) {
                result.Add(UncertaintySource.BodIndicatorValues);
            }
            return result;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            data.BurdensOfDisease = [.. subsetManager.AllBurdensOfDisease
                .Where(r => r.Population == null
                    || data.SelectedPopulation.Code == "Generated"
                    || r.Population == data.SelectedPopulation
                )];
            data.BodIndicatorConversions = [.. subsetManager.AllBodIndicatorConversions];
            data.BodIndicatorValueModels = [.. data.BurdensOfDisease.Select(BodIndicatorValueCalculatorFactory.Create)];
        }

        protected override void summarizeActionResult(IBurdensOfDiseaseActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new BurdensOfDiseaseSummarizer();
            summarizer.Summarize(_project.ActionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        /// <summary>
        /// Load and resample distribution voor burden of disease indicator values.
        /// </summary>
        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            if (data.BodIndicatorValueModels != null && data.BodIndicatorValueModels?.Count > 0) {
                foreach (var model in data.BodIndicatorValueModels) {
                    if (factorialSet.Contains(UncertaintySource.BodIndicatorValues)) {
                        localProgress.Update("Resampling burden of disease indicator values.");
                        var random = uncertaintySourceGenerators[UncertaintySource.BodIndicatorValues];
                        model.ResampleModelParameters(random);
                    }
                }
            }
            localProgress.Update(100);
        }
    }
}
