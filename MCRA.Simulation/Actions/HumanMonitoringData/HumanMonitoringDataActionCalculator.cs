using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.HumanMonitoringData {
    [ActionType(ActionType.HumanMonitoringData)]
    public class HumanMonitoringDataActionCalculator : ActionCalculatorBase<IHumanMonitoringDataActionResult> {

        public HumanMonitoringDataActionCalculator(ProjectDto project) : base(project) {
        }
        protected override void verify() {
            _actionDataSelectionRequirements[ScopingType.HumanMonitoringIndividualProperties].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.HumanMonitoringIndividualPropertyValues].AllowEmptyScope = true;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringSurveys][ScopingType.Populations].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringIndividualPropertyValues][ScopingType.HumanMonitoringIndividuals].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringSampleAnalyses][ScopingType.HumanMonitoringSamples].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringSampleConcentrations][ScopingType.HumanMonitoringSampleAnalyses].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringSampleConcentrations][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HumanMonitoringAnalyticalMethodCompounds][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new HumanMonitoringDataSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (_project.UncertaintyAnalysisSettings.ResampleHBMIndividuals) {
                result.Add(UncertaintySource.HbmIndividuals);
            }
            return result;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var settings = new HumanMonitoringDataModuleSettings(_project);
            var surveys = subsetManager.AllHumanMonitoringSurveys;

            if (!surveys?.Any() ?? true) {
                throw new Exception("No human monitoring survey selected");
            } else if (surveys.Count > 1) {
                throw new Exception("Multiple human monitoring surveys selected");
            }

            var survey = surveys.Single();

            // Get selected sampling methods
            var samplingMethods = subsetManager.GetAllHumanMonitoringSamplingMethods();
            if (settings.SamplingMethodCodes?.Any() ?? false) {
                var selectedSamplingMethodCodes = settings.SamplingMethodCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
                samplingMethods = samplingMethods.Where(r => selectedSamplingMethodCodes.Contains(r.Code)).ToList();
            }
            if (!samplingMethods.Any()) {
                throw new Exception("Specified sampling method not found!");
            }

            // Get individuals
            var availableIndividuals = subsetManager
                .AllHumanMonitoringIndividuals
                .Where(r => r.CodeFoodSurvey.Equals(survey.Code, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Create individual (subset) filters
            var individualsSubsetCalculator = new IndividualsSubsetFiltersBuilder();
            var individualFilters = individualsSubsetCalculator.Create(
                data.SelectedPopulation,
                subsetManager.AllHumanMonitoringIndividualProperties,
                settings.MatchHbmIndividualSubsetWithPopulation,
                settings.SelectedHbmSurveySubsetProperties
            );

            // Get the individuals from individual subset
            var individuals = IndividualsSubsetCalculator
                .ComputeIndividualsSubset(
                    availableIndividuals,
                    individualFilters
                );

            // Overwrite sampling weight
            if (!settings.UseHbmSamplingWeights) {
                foreach (var individual in individuals) {
                    individual.SamplingWeight = 1D;
                }
            }

            // Get the HBM samples
            var allSamples = subsetManager.AllHumanMonitoringSamples
                .Where(r => individuals.Contains(r.Individual))
                .Where(r => samplingMethods.Contains(r.SamplingMethod))
                .ToList();

            var excludedSubstanceMethods = settings.ExcludeSubstancesFromSamplingMethod ? settings.ExcludedSubstancesFromSamplingMethodSubset
                   .GroupBy(c => c.SamplingMethodCode)
                   .ToDictionary(c => c.Key, c => c.Select(n => n.SubstanceCode).ToList())
                   : new();

            var samples = allSamples;
            if (settings.UseCompleteAnalysedSamples) {
                var completeSamplesPerSamplingMethod = new Dictionary<HumanMonitoringSamplingMethod, List<HumanMonitoringSample>>();
                foreach (var method in samplingMethods) {
                    excludedSubstanceMethods.TryGetValue(method.Code, out List<string> excludedSubstances);
                    var allSubstances = allSamples
                        .Where(c => c.SamplingMethod == method)
                        .SelectMany(c => c.SampleAnalyses.SelectMany(am => am.AnalyticalMethod.AnalyticalMethodCompounds.Keys))
                        .Distinct()
                        .Where(s => !excludedSubstances?.Contains(s.Code) ?? true)
                        .ToList();

                    var completeSamples = allSamples
                        .Where(c => c.SamplingMethod == method)
                        .SelectMany(c =>
                            c.SampleAnalyses.Select(am => am.AnalyticalMethod.AnalyticalMethodCompounds.Keys)
                            .Where(r => allSubstances.Except(r).Count() == 0),
                            (c, k) => c)
                        .ToList();

                    completeSamplesPerSamplingMethod[method] = completeSamples;
                }

                // Take intercept on individuals for all sampling methods
                List<int> allCompleteIndividuals = null;
                foreach (var comleteSamples in completeSamplesPerSamplingMethod.Values) {
                    if (allCompleteIndividuals == null) {
                        allCompleteIndividuals = comleteSamples.Select(c => c.Individual.Id).ToList();
                    } else {
                        List<int> completeIndividuals = comleteSamples.Select(c => c.Individual.Id).ToList();
                        allCompleteIndividuals = allCompleteIndividuals.Intersect(completeIndividuals).ToList();
                    }
                }
                foreach (var kv in completeSamplesPerSamplingMethod) {
                    completeSamplesPerSamplingMethod[kv.Key] = kv.Value.Where(s => allCompleteIndividuals.Contains(s.Individual.Id)).ToList();
                }
                samples = completeSamplesPerSamplingMethod.SelectMany(d => d.Value).ToList();
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
            var summarizer = new HumanMonitoringDataSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
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
                    var hbmSampleSubstanceDictionary = collection.HumanMonitoringSampleSubstanceRecords.ToLookup(r => r.Individual);
                    var hbmSampleSubstanceRecords = data.HbmIndividuals
                        .SelectMany((c, ix) => {
                            var records = hbmSampleSubstanceDictionary[c].Select(r => r.Clone()).ToList();
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