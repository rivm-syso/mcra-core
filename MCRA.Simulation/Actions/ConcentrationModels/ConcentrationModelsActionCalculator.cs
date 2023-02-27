using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationDistributionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using MCRA.Simulation.Calculators.SampleCompoundCollections.MissingValueImputation;
using MCRA.Simulation.Calculators.SampleCompoundCollections.NonDetectsImputation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.ConcentrationModels {

    [ActionType(ActionType.ConcentrationModels)]
    public sealed class ConcentrationModelsActionCalculator : ActionCalculatorBase<ConcentrationModelsActionResult> {

        public ConcentrationModelsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isCumulative = _project.AssessmentSettings.MultipleSubstances && _project.AssessmentSettings.Cumulative;

            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = _project.AssessmentSettings.MultipleSubstances;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = _project.AssessmentSettings.MultipleSubstances;

            _actionInputRequirements[ActionType.Effects].IsRequired = isCumulative;
            _actionInputRequirements[ActionType.Effects].IsVisible = isCumulative;

            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = isCumulative;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = isCumulative;

            _actionInputRequirements[ActionType.ConcentrationLimits].IsVisible = _project.ConcentrationModelSettings.IsFallbackMrl;
            _actionInputRequirements[ActionType.ConcentrationLimits].IsRequired = false;

            var restrictLorImputationToAuthorisedUses = _project.ConcentrationModelSettings.RestrictLorImputationToAuthorisedUses;
            _actionInputRequirements[ActionType.SubstanceAuthorisations].IsVisible = restrictLorImputationToAuthorisedUses;
            _actionInputRequirements[ActionType.SubstanceAuthorisations].IsRequired = restrictLorImputationToAuthorisedUses;

            var useOccurrenceFrequencies =  _project.AgriculturalUseSettings.UseAgriculturalUseTable;
            _actionInputRequirements[ActionType.OccurrenceFrequencies].IsVisible = useOccurrenceFrequencies;
            _actionInputRequirements[ActionType.OccurrenceFrequencies].IsRequired = useOccurrenceFrequencies;

            var useConcentrationDistributions = _project.ConcentrationModelSettings.DefaultConcentrationModel == ConcentrationModelType.SummaryStatistics;
            _actionInputRequirements[ActionType.ConcentrationDistributions].IsVisible = useConcentrationDistributions;
            _actionInputRequirements[ActionType.ConcentrationDistributions].IsRequired = false;

            var isTotalDietStudy = _project.AssessmentSettings.TotalDietStudy;
            _actionInputRequirements[ActionType.TotalDietStudyCompositions].IsVisible = isTotalDietStudy;
            _actionInputRequirements[ActionType.TotalDietStudyCompositions].IsRequired = false;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new ConcentrationModelsSettingsManager();
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (_project.UncertaintyAnalysisSettings.ReSampleConcentrations) {
                if (!_project.ConcentrationModelSettings.IsSampleBased
                    || _project.UncertaintyAnalysisSettings.IsParametric
                ) {
                    result.Add(UncertaintySource.ConcentrationModelling);
                }
                result.Add(UncertaintySource.ConcentrationMissingValueImputation);
                result.Add(UncertaintySource.ConcentrationNonDetectImputation);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ConcentrationModelsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override ConcentrationModelsActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            var settings = new ConcentrationModelsModuleSettings(_project);
            var substances = settings.IsMultipleSubstances
                ? data.ActiveSubstances
                : data.ModelledSubstances;

            // Create compound residue collections from sample compound collections
            var compoundResidueCollectionsBuilder = new CompoundResidueCollectionsBuilder(
                settings.RestrictLorImputationToAuthorisedUses
            );
            var compoundResidueCollections = compoundResidueCollectionsBuilder
                .Create(
                    substances,
                    data.ActiveSubstanceSampleCollections?.Values,
                    data.OccurrenceFractions,
                    data.SubstanceAuthorisations
                );

            // Select only TDS compositions that are found in conversion algorithm
            if (settings.TotalDietStudy) {
                if ((data.TdsFoodCompositions?.Any() ?? false)
                    && (data.ConcentrationDistributions?.Any() ?? false)
                ) {
                    var concentrationDistributionCalculator = new ConcentrationDistributionCalculator(
                        data.ConcentrationDistributions
                    );
                    foreach (var substance in substances) {
                        foreach (var compositions in data.TdsFoodCompositions) {
                            if (compoundResidueCollections.TryGetValue((compositions.Key, substance), out var crc)) {
                                crc.StandardDeviation = concentrationDistributionCalculator.GetStandardDeviation(compositions.ToList(), substance);
                            }
                        }
                    }
                }
            }

            // Create concentration models per food/substance
            var concentrationModelsBuilder = new ConcentrationModelsBuilder(settings);

            IDictionary<(Food, Compound), double> occurrenceFrequencies = null;
            if (data.OccurrenceFractions != null) {
                occurrenceFrequencies = new Dictionary<(Food, Compound), double>();
                foreach (var item in data.OccurrenceFractions) {
                    occurrenceFrequencies.Add(item.Key, item.Value.OccurrenceFrequency);
                }
            }
            var concentrationModels = concentrationModelsBuilder.Create(
                data.ModelledFoods,
                substances,
                compoundResidueCollections,
                data.ConcentrationDistributions,
                data.MaximumConcentrationLimits,
                occurrenceFrequencies,
                data.ConcentrationUnit,
                progressReport
            );

            ICollection<MarginalOccurrencePattern> simulatedOccurrencePatterns = null;
            Dictionary<Food, ConcentrationModel> cumulativeConcentrationModels = null;
            ICollection<SampleCompoundCollection> monteCarloSubstanceSampleCollections = null;
            if (settings.IsSampleBased) {

                // Clone sample compound collections and impute NDs/MVs
                monteCarloSubstanceSampleCollections = data.ActiveSubstanceSampleCollections?.Values
                    .Select(r => r.Clone())
                    .ToList();

                // Censored value imputation
                var censoredValueImputationCalculator = new CensoredValuesImputationCalculator(settings);
                censoredValueImputationCalculator.ReplaceCensoredValues(
                    monteCarloSubstanceSampleCollections,
                    concentrationModels,
                    Simulation.IsBackwardCompatibilityMode 
                        ? _project.MonteCarloSettings.RandomSeed
                        : RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.CM_NonDetectsImputation),
                    progressReport
                );

                // Missing value imputation
                var missingValueImputationCalculator = new MissingvalueImputationCalculator(settings);
                if (settings.ImputeMissingValues) {
                    missingValueImputationCalculator.ImputeMissingValues(
                        monteCarloSubstanceSampleCollections,
                        concentrationModels,
                        data.CorrectedRelativePotencyFactors,
                        Simulation.IsBackwardCompatibilityMode
                            ? _project.MonteCarloSettings.RandomSeed
                            : RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.CM_MissingValueImputation),
                        progressReport
                    );
                } else {
                    missingValueImputationCalculator.ReplaceImputeMissingValuesByZero(monteCarloSubstanceSampleCollections, progressReport);
                }
                var agriculturalUseSettings = new OccurrencePatternsFromFindingsCalculatorSettings() ;
                var occurrencePatternCalculator = new OccurrencePatternsFromFindingsCalculator(agriculturalUseSettings);
                simulatedOccurrencePatterns = occurrencePatternCalculator.Compute(
                    data.ModelledFoods,
                    monteCarloSubstanceSampleCollections?.ToDictionary(r => r.Food),
                    progressReport
                );

                // Create cumulative concentration models
                if (substances.Count > 1 && settings.Cumulative ) {
                    var cumulativeCompoundResidueCollectionsBuilder = new CumulativeCompoundResidueCollectionsBuilder();
                    var cumulativeCompoundResidueCollections = cumulativeCompoundResidueCollectionsBuilder.Create(
                        monteCarloSubstanceSampleCollections,
                        data.CumulativeCompound,
                        data.CorrectedRelativePotencyFactors
                    );
                    var cumulativeConcentrationModelsCalculator = new CumulativeConcentrationModelsBuilder(settings);
                    cumulativeConcentrationModels = cumulativeConcentrationModelsCalculator.Create(
                        data.ModelledFoods,
                        cumulativeCompoundResidueCollections,
                        data.CumulativeCompound,
                        data.ConcentrationUnit
                    );
                }
            }

            localProgress.Update(100);
            return new ConcentrationModelsActionResult() {
                ConcentrationModels = concentrationModels,
                CumulativeConcentrationModels = cumulativeConcentrationModels,
                CompoundResidueCollections = compoundResidueCollections,
                SimulatedOccurrencePatterns = simulatedOccurrencePatterns,
                MonteCarloSubstanceSampleCollections = monteCarloSubstanceSampleCollections
            };
        }

        protected override void updateSimulationData(ActionData data, ConcentrationModelsActionResult result) {
            data.ConcentrationModels = result.ConcentrationModels;
            data.CumulativeConcentrationModels = result.CumulativeConcentrationModels;
            data.CompoundResidueCollections = result.CompoundResidueCollections;
            data.MonteCarloSubstanceSampleCollections = result.MonteCarloSubstanceSampleCollections;
        }

        protected override void summarizeActionResult(ConcentrationModelsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing concentration models", 0);
            var summarizer = new ConcentrationModelsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override ConcentrationModelsActionResult runUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(90);
            var settings = new ConcentrationModelsModuleSettings(_project);
            var substances = data.ModelledSubstances;

            var substanceResidueCollections = data.CompoundResidueCollections;
            if (settings.ReSampleConcentrations
                && factorialSet.Contains(UncertaintySource.ConcentrationModelling)
            ) {
                if (settings.IsSampleBased) {
                    // Recreate from bootstrapped sample compound collections
                    var compoundResidueCollectionsBuilder = new CompoundResidueCollectionsBuilder(
                        settings.RestrictLorImputationToAuthorisedUses
                    );
                    substanceResidueCollections = compoundResidueCollectionsBuilder
                        .Create(
                            substances,
                            data.ActiveSubstanceSampleCollections?.Values,
                            data.OccurrenceFractions,
                            data.SubstanceAuthorisations
                        );
                } else if (!settings.IsSampleBased) {
                    substanceResidueCollections = CompoundResidueCollectionsBuilder
                        .Resample(
                            data.CompoundResidueCollections,
                            uncertaintySourceGenerators[UncertaintySource.ConcentrationModelling]
                        );
                }
            }

            var result = new ConcentrationModelsActionResult();
            var substanceConcentrationModels = data.ConcentrationModels
                .Where(r => r.Value.Compound != data.CumulativeCompound)
                .ToDictionary(r => r.Key, r => r.Value);

            // Create concentration models per food/substance
            var concentrationModelsBuilder = new ConcentrationModelsBuilder(settings);
            var newCompoundConcentrationModels = concentrationModelsBuilder
                .CreateUncertain(
                    substanceConcentrationModels,
                    substanceResidueCollections,
                    data.ConcentrationDistributions,
                    data.MaximumConcentrationLimits,
                    data.OccurrenceFractions,
                    settings.ReSampleConcentrations,
                    settings.IsParametric,
                    data.ConcentrationUnit,
                    Simulation.IsBackwardCompatibilityMode
                        ? _project.MonteCarloSettings.RandomSeed
                        : factorialSet.Contains(UncertaintySource.ConcentrationModelling)
                            ? (int?)uncertaintySourceGenerators[UncertaintySource.ConcentrationModelling].Seed
                            : null
                );

            // Clone sample compound collections and impute NDs/MVs
            var monteCarloSubstanceSampleCollections = data.MonteCarloSubstanceSampleCollections;

            if (settings.IsSampleBased) {

                if (settings.ReSampleConcentrations) {

                    // Clone sample compound collections and impute NDs/MVs
                    monteCarloSubstanceSampleCollections = data.ActiveSubstanceSampleCollections?.Values
                        .Select(r => r.Clone()).ToList();

                    // Censored values imputation
                    var nonDetectsImputationCalculator = new CensoredValuesImputationCalculator(settings);
                    nonDetectsImputationCalculator.ReplaceCensoredValues(
                        monteCarloSubstanceSampleCollections,
                        newCompoundConcentrationModels,
                        Simulation.IsBackwardCompatibilityMode
                            ? _project.MonteCarloSettings.RandomSeed
                            : factorialSet.Contains(UncertaintySource.ConcentrationNonDetectImputation)
                                ? uncertaintySourceGenerators[UncertaintySource.ConcentrationNonDetectImputation].Seed
                                : RandomUtils.CreateSeed(uncertaintySourceGenerators[UncertaintySource.ConcentrationNonDetectImputation].Seed, (int)RandomSource.CM_NonDetectsImputation),
                        progressReport
                    );

                    // Missing value imputation
                    var missingValueImputationCalculator = new MissingvalueImputationCalculator(settings);
                    if (settings.ImputeMissingValues) {
                        missingValueImputationCalculator.ImputeMissingValues(
                            monteCarloSubstanceSampleCollections,
                            newCompoundConcentrationModels,
                            data.CorrectedRelativePotencyFactors,
                            Simulation.IsBackwardCompatibilityMode
                                ? _project.MonteCarloSettings.RandomSeed
                                : factorialSet.Contains(UncertaintySource.ConcentrationMissingValueImputation)
                                    ? uncertaintySourceGenerators[UncertaintySource.ConcentrationMissingValueImputation].Seed
                                    : RandomUtils.CreateSeed(uncertaintySourceGenerators[UncertaintySource.ConcentrationMissingValueImputation].Seed, (int)RandomSource.CM_MissingValueImputation),
                            progressReport
                        );
                    } else {
                        missingValueImputationCalculator.ReplaceImputeMissingValuesByZero(monteCarloSubstanceSampleCollections, progressReport);
                    }
                }

                if (settings.Cumulative && substances.Count > 1) {
                    localProgress.Update("Initializing cumulative concentration models", 28);
                    var concentrationModellingProgressState2 = progressReport.NewProgressState(10D);
                    var cumulativeCompoundResidueCollectionBuilder = new CumulativeCompoundResidueCollectionsBuilder();
                    var cumulativeCompoundResidueCollection = cumulativeCompoundResidueCollectionBuilder.Create(monteCarloSubstanceSampleCollections, data.CumulativeCompound, data.CorrectedRelativePotencyFactors);
                    var cumulativeConcentrationModelsBuilder = new CumulativeConcentrationModelsBuilder(settings);
                    if (settings.IsParametric) {
                        result.CumulativeConcentrationModels = cumulativeConcentrationModelsBuilder
                            .CreateUncertain(
                                data.ModelledFoods,
                                cumulativeCompoundResidueCollection,
                                data.CumulativeCompound,
                                settings.ReSampleConcentrations,
                                settings.IsParametric,
                                data.ConcentrationUnit,
                                Simulation.IsBackwardCompatibilityMode
                                    ? _project.MonteCarloSettings.RandomSeed
                                    : factorialSet.Contains(UncertaintySource.ConcentrationModelling)
                                        ? (int?)uncertaintySourceGenerators[UncertaintySource.ConcentrationModelling].Seed
                                        : null
                            );
                    } else {
                        // This is wrong!!! We should create the cumulative concentration models based on the bootstrapped data!
                        // Kept for reproducibility of old results, but should be fixed in the near future.
                        result.CumulativeConcentrationModels = data.CumulativeConcentrationModels;
                    }
                }
            }

            result.MonteCarloSubstanceSampleCollections = monteCarloSubstanceSampleCollections;
            result.CompoundResidueCollections = substanceResidueCollections;
            result.ConcentrationModels = newCompoundConcentrationModels;
            return result;
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, ConcentrationModelsActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ConcentrationModelsSummarizer();
            summarizer.SummarizeUncertain(_project, actionResult, header);
            localProgress.Update(100);
        }

        protected override void updateSimulationDataUncertain(ActionData data, ConcentrationModelsActionResult result) {
            data.ConcentrationModels = result.ConcentrationModels;
            data.CumulativeConcentrationModels = result.CumulativeConcentrationModels;
            data.CompoundResidueCollections = result.CompoundResidueCollections;
            data.MonteCarloSubstanceSampleCollections = result.MonteCarloSubstanceSampleCollections;
        }
    }
}
