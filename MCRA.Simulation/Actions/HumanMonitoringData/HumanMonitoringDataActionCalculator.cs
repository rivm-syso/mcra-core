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
            var samples = subsetManager.AllHumanMonitoringSamples
                .Where(r => individuals.Contains(r.Individual))
                .Where(r => samplingMethods.Contains(r.SamplingMethod))
                .ToList();

            // Create sample substance collections
            data.HbmSampleSubstanceCollections = HumanMonitoringSampleSubstanceCollectionsBuilder
                .Create(
                    data.AllCompounds,
                    samples,
                    survey,
                    progressState
                );

            data.HbmSurveys = surveys;
            data.HbmIndividuals = individuals;
            data.HbmSamples = samples;
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

                data.HbmSamples = data.HbmSamples
                    .Where(r => data.HbmIndividuals.Contains(r.Individual))
                    .Where(r => data.HbmSamplingMethods.Contains(r.SamplingMethod))
                    .ToList();

                var hbmSampleSubstanceCollections = new List<HumanMonitoringSampleSubstanceCollection>();
                foreach (var collection in data.HbmSampleSubstanceCollections) {
                    var hbmSampleSubstanceDictionary = collection.HumanMonitoringSampleSubstanceRecords.ToLookup(r => r.Individual);
                    var hbmSampleSubstanceRecords = data.HbmIndividuals
                        .SelectMany((c, ix) => {
                            var records = hbmSampleSubstanceDictionary[c];
                            foreach (var item in records) {
                                var record = item.Clone();
                                record.SimulatedIndividualId = ix;
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