using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Management;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.OccurrenceFrequenciesCalculation;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.OccurrenceFrequencies {

    [ActionType(ActionType.OccurrenceFrequencies)]
    public class OccurrenceFrequenciesActionCalculator : ActionCalculatorBase<OccurrenceFrequenciesActionResult> {
        private OccurrenceFrequenciesModuleConfig ModuleConfig => (OccurrenceFrequenciesModuleConfig)_moduleSettings;

        public OccurrenceFrequenciesActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = false;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = ModuleConfig.MultipleSubstances;
            _actionDataLinkRequirements[ScopingType.OccurrenceFrequencies][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.OccurrenceFrequencies][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new OccurrencePatternsSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new OccurrenceFrequenciesSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_isCompute, _project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var settings = new OccurrenceFractionsCalculatorSettings(ModuleConfig);
            var occurrenceFractionsBuilder = new OccurrenceFractionsBuilder(settings);
            var occurrenceFrequencies = subsetManager.AllOccurrenceFrequencies;
            data.OccurrenceFractions = occurrenceFractionsBuilder.Create(occurrenceFrequencies);
            localProgress.Update(100);
        }

        protected override OccurrenceFrequenciesActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var result = new OccurrenceFrequenciesActionResult();
            var settings = new OccurrenceFractionsCalculatorSettings(ModuleConfig);
            var occurrenceFrequenciesCalculator = new OccurrenceFractionsCalculator(settings);
            if (data.RawAgriculturalUses != null) {
                result.OccurrenceFrequencies = occurrenceFrequenciesCalculator
                    .ComputeLocationBased(
                        data.AllFoods,
                        data.ActiveSubstances,
                        data.RawAgriculturalUses,
                        data.SampleOriginInfos
                    );
            } else {
                result.OccurrenceFrequencies = occurrenceFrequenciesCalculator
                    .Compute(
                        data.AllFoods,
                        data.ActiveSubstances,
                        data.MarginalOccurrencePatterns
                    );
            }
            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, OccurrenceFrequenciesActionResult result) {
            if (result != null) {
                data.OccurrenceFractions = result.OccurrenceFrequencies;
            }
        }

        protected override OccurrenceFrequenciesActionResult runUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewCompositeState(100);
            if (factorialSet.Contains(UncertaintySource.Concentrations) && ModuleConfig.RecomputeOccurrencePatterns) {
                return run(data, localProgress);
            }
            localProgress.MarkCompleted();
            return null;
        }

        protected override void updateSimulationDataUncertain(ActionData data, OccurrenceFrequenciesActionResult result) {
            updateSimulationData(data, result);
        }

        protected override void summarizeActionResult(OccurrenceFrequenciesActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new OccurrenceFrequenciesSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, OccurrenceFrequenciesActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new OccurrenceFrequenciesSummarizer();
            summarizer.SummarizeUncertain(data, header);
            localProgress.Update(100);
        }
    }
}
