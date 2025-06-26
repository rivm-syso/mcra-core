using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.CompleteSamplesCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.HumanMonitoringData {
    [ActionType(ActionType.HumanMonitoringData)]
    public class HumanMonitoringDataActionCalculator : ActionCalculatorBase<IHumanMonitoringDataActionResult> {
        private HumanMonitoringDataModuleConfig ModuleConfig => (HumanMonitoringDataModuleConfig)_moduleSettings;

        public HumanMonitoringDataActionCalculator(ProjectDto project) : base(project) {
        }
        protected override void verify() {
            _actionDataSelectionRequirements[ScopingType.HumanMonitoringIndividualProperties].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.HumanMonitoringIndividualPropertyValues].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.HumanMonitoringTimepoints].AllowEmptyScope = true;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringSurveys][ScopingType.Populations].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringTimepoints][ScopingType.HumanMonitoringSurveys].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringIndividualPropertyValues][ScopingType.HumanMonitoringIndividuals].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringSampleAnalyses][ScopingType.HumanMonitoringSamples].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringSampleConcentrations][ScopingType.HumanMonitoringSampleAnalyses].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringSampleConcentrations][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringAnalyticalMethodCompounds][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new HumanMonitoringDataSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleHbmIndividuals) {
                result.Add(UncertaintySource.HbmIndividuals);
            }
            return result;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var surveys = subsetManager.AllHumanMonitoringSurveys;
            if (!surveys?.Any() ?? true) {
                throw new Exception("No human monitoring survey selected");
            } else if (surveys.Count > 1) {
                throw new Exception("Multiple human monitoring surveys selected");
            }
            var survey = surveys.Single();

            // Get selected sampling methods
            var samplingMethods = subsetManager.GetAllHumanMonitoringSamplingMethods();
            if (ModuleConfig.CodesHumanMonitoringSamplingMethods?.Count > 0) {
                var selectedSamplingMethodCodes = ModuleConfig.CodesHumanMonitoringSamplingMethods
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                samplingMethods = samplingMethods
                    .Where(r => selectedSamplingMethodCodes.Contains(r.Code))
                    .ToList();
            }
            if (!samplingMethods.Any()) {
                throw new Exception("Specified sampling method not found!");
            }

            var timepointCodes = survey.Timepoints.Select(t => t.Code);
            if (ModuleConfig.FilterRepeatedMeasurements
                && (ModuleConfig.RepeatedMeasurementTimepointCodes?.Count > 0)
            ) {
                timepointCodes = timepointCodes
                    .Where(ModuleConfig.RepeatedMeasurementTimepointCodes.Contains)
                    .ToArray();
            }
            if (!timepointCodes.Any()) {
                throw new Exception("No measurements / time points selected!");
            }

            var individuals = IndividualsSubsetCalculator.GetIndividualSubsets(
                subsetManager.AllHumanMonitoringIndividuals,
                subsetManager.AllHumanMonitoringIndividualProperties,
                data.SelectedPopulation,
                survey.Code,
                ModuleConfig.MatchHbmIndividualSubsetWithPopulation,
                ModuleConfig.SelectedHbmSurveySubsetProperties,
                ModuleConfig.UseHbmSamplingWeights
            );

            // Get the HBM samples
            var allSamples = subsetManager.AllHumanMonitoringSamples
                .Where(r => individuals.Contains(r.Individual))
                .Where(r => samplingMethods.Contains(r.SamplingMethod))
                .Where(r => timepointCodes.Contains(r.DayOfSurvey))
                .ToList();

            var excludedSubstanceMethods = ModuleConfig.ExcludeSubstancesFromSamplingMethod
                ? ModuleConfig.ExcludedSubstancesFromSamplingMethodSubset
                   .GroupBy(c => c.SamplingMethodCode)
                   .ToDictionary(c => c.Key, c => c.Select(n => n.SubstanceCode).ToList())
                : [];

            var samples = allSamples;
            if (ModuleConfig.UseCompleteAnalysedSamples) {
                samples = CompleteSamplesCalculator
                    .FilterCompleteAnalysedSamples(
                        allSamples,
                        samplingMethods,
                        excludedSubstanceMethods
                    )
                    .ToList();
                individuals = samples
                    .Select(r => r.Individual)
                    .ToHashSet();
            }

            // Create sample substance collections
            data.HbmSampleSubstanceCollections = HumanMonitoringSampleSubstanceCollectionsBuilder
                .Create(
                    data.AllCompounds,
                    samples,
                    survey,
                    excludedSubstanceMethods,
                    progressState
                );

            data.HbmSurveys = surveys;
            data.HbmIndividuals = individuals;
            data.HbmAllSamples = allSamples;
            data.HbmSamplingMethods = samplingMethods;
        }

        protected override void summarizeActionResult(IHumanMonitoringDataActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing human monitoring data", 0);
            var summarizer = new HumanMonitoringDataSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void updateSimulationDataUncertain(ActionData data, IHumanMonitoringDataActionResult result) {
            updateSimulationData(data, result);
        }

        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            // Bootstrap hbm individuals
            if (factorialSet.Contains(UncertaintySource.HbmIndividuals)) {
                localProgress.Update("Resampling hbm individuals");
                data.HbmIndividuals = data.HbmIndividuals
                    .Resample(uncertaintySourceGenerators[UncertaintySource.HbmIndividuals])
                    .ToList();

                data.HbmAllSamples = data.HbmAllSamples
                    .Where(r => data.HbmIndividuals.Contains(r.Individual))
                    .Where(r => data.HbmSamplingMethods.Contains(r.SamplingMethod))
                    .ToList();

                var hbmSampleSubstanceCollections = new List<HumanMonitoringSampleSubstanceCollection>();
                foreach (var collection in data.HbmSampleSubstanceCollections) {
                    var hbmSampleSubstanceDictionary = collection.HumanMonitoringSampleSubstanceRecords.ToLookup(r => r.Individual.Id);
                    var hbmSampleSubstanceRecords = data.HbmIndividuals
                        .SelectMany((c, ix) => {
                            var records = hbmSampleSubstanceDictionary[c.Id].Select(r => r.Clone()).ToList();
                            foreach (var item in records) {
                                item.SimulatedIndividualId = ix;
                            }
                            return records;
                        })
                        .ToList();
                    var hbmSampleSubstanceCollection = new HumanMonitoringSampleSubstanceCollection(
                            collection.SamplingMethod,
                            hbmSampleSubstanceRecords,
                            collection.ConcentrationUnit,
                            collection.ExpressionType,
                            collection.TriglycConcentrationUnit,
                            collection.CholestConcentrationUnit,
                            collection.LipidConcentrationUnit,
                            collection.CreatConcentrationUnit
                        );
                    hbmSampleSubstanceCollections.Add(hbmSampleSubstanceCollection);
                }

                // Create sample substance collections
                data.HbmSampleSubstanceCollections = hbmSampleSubstanceCollections;
            }
            localProgress.Update(100);
        }
    }
}