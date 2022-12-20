using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ActiveSubstanceAllocation;
using MCRA.Simulation.Calculators.FocalCommodityCombinationsBuilder;
using MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation;
using MCRA.Simulation.Calculators.FoodExtrapolationsCalculation;
using MCRA.Simulation.Calculators.SampleCompoundCollections;
using MCRA.Simulation.Calculators.SampleOriginCalculation;
using MCRA.Simulation.Filters.FoodSampleFilters;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.DateTimes;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Actions.Concentrations {

    [ActionType(ActionType.Concentrations)]
    public sealed class ConcentrationsActionCalculator : ActionCalculatorBase<IConcentrationsActionResult> {

        public ConcentrationsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var useFocalCommodity = _project.AssessmentSettings.FocalCommodity
                && !(_project.ConcentrationModelSettings.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.MeasurementRemoval
                || _project.ConcentrationModelSettings.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue);
            _actionInputRequirements[ActionType.FocalFoodConcentrations].IsRequired = useFocalCommodity;
            _actionInputRequirements[ActionType.FocalFoodConcentrations].IsVisible = useFocalCommodity;

            var regionSubsetDefinition = _project.SamplesSubsetDefinitions?.FirstOrDefault(r => r.IsRegionSubset());
            var alignSampleSubsetWithPopulation = _project.SubsetSettings.SampleSubsetSelection 
                && (_project.PeriodSubsetDefinition.AlignSampleDateSubsetWithPopulation
                    || _project.PeriodSubsetDefinition.AlignSampleSeasonSubsetWithPopulation
                    || _project.LocationSubsetDefinition.AlignSubsetWithPopulation
                    || (regionSubsetDefinition?.AlignSubsetWithPopulation ?? false)
                );
            _actionInputRequirements[ActionType.Populations].IsRequired = alignSampleSubsetWithPopulation;
            _actionInputRequirements[ActionType.Populations].IsVisible = alignSampleSubsetWithPopulation;

            var useComplexResidueDefinitions = _project.ConcentrationModelSettings.UseComplexResidueDefinitions;
            _actionInputRequirements[ActionType.SubstanceConversions].IsVisible = useComplexResidueDefinitions;
            _actionInputRequirements[ActionType.SubstanceConversions].IsRequired = useComplexResidueDefinitions;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = useComplexResidueDefinitions;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = useComplexResidueDefinitions;
            var isExtrapolation = _project.ConcentrationModelSettings.ExtrapolateConcentrations;
            _actionInputRequirements[ActionType.FoodExtrapolations].IsRequired = isExtrapolation;
            _actionInputRequirements[ActionType.FoodExtrapolations].IsVisible = isExtrapolation;
            var isMrlInput = _project.ConcentrationModelSettings.FilterConcentrationLimitExceedingSamples
                || (isExtrapolation && _project.ConcentrationModelSettings.ConsiderMrlForExtrapolations)
                || (_project.AssessmentSettings.FocalCommodity && _project.ConcentrationModelSettings.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue);
            _actionInputRequirements[ActionType.ConcentrationLimits].IsRequired = isMrlInput;
            _actionInputRequirements[ActionType.ConcentrationLimits].IsVisible = isMrlInput;
            var isWaterImputation = _project.ConcentrationModelSettings.ImputeWaterConcentrations;
            var substanceAuthorisations = (isExtrapolation && _project.ConcentrationModelSettings.ConsiderAuthorisationsForExtrapolations)
                || (useComplexResidueDefinitions && _project.ConcentrationModelSettings.ConsiderAuthorisationsForSubstanceConversion)
                || (isWaterImputation && _project.ConcentrationModelSettings.RestrictWaterImputationToAuthorisedUses);
            _actionInputRequirements[ActionType.SubstanceAuthorisations].IsRequired = substanceAuthorisations;
            _actionInputRequirements[ActionType.SubstanceAuthorisations].IsVisible = substanceAuthorisations;
            var isWaterImputationWithPotencyRestrition = isWaterImputation
                && _project.ConcentrationModelSettings.RestrictWaterImputationToMostPotentSubstances;
            var rpfVisible = isWaterImputationWithPotencyRestrition ||
                (useComplexResidueDefinitions &&
                _project.ConcentrationModelSettings.SubstanceTranslationAllocationMethod == SubstanceTranslationAllocationMethod.UseMostToxic);
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = rpfVisible;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = rpfVisible;
            var useDeterministicSubstanceConversions = _project.AssessmentSettings.FocalCommodity
                && _project.ConcentrationModelSettings.UseDeterministicSubstanceConversionsForFocalCommodity;
            _actionInputRequirements[ActionType.DeterministicSubstanceConversionFactors].IsVisible = useDeterministicSubstanceConversions;
            _actionInputRequirements[ActionType.DeterministicSubstanceConversionFactors].IsRequired = useDeterministicSubstanceConversions;

            // Data selection requirements
            _actionDataSelectionRequirements[ScopingType.AnalyticalMethods].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.FoodSamples].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.SampleAnalyses].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.FoodSampleProperties].AllowEmptyScope = true;
            _actionDataSelectionRequirements[ScopingType.FoodSamplePropertyValues].AllowEmptyScope = true;

            // Data linking requirements
            _actionDataLinkRequirements[ScopingType.FoodSamples][ScopingType.Foods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ConcentrationsPerSample][ScopingType.SampleAnalyses].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.SampleAnalyses][ScopingType.AnalyticalMethods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.AnalyticalMethodCompounds][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.AnalyticalMethodCompounds][ScopingType.AnalyticalMethods].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ConcentrationsPerSample][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.FoodSamplePropertyValues][ScopingType.FoodSamples].AlertTypeMissingData = AlertType.Notification;
        }

        public override bool CheckDataDependentSettings(ICompiledLinkManager linkManager) {
            if (_project.ConcentrationModelSettings.ImputeWaterConcentrations) {
                var codeWater = _project.ConcentrationModelSettings.CodeWater;
                if (!linkManager.GetCodesInScope(ScopingType.Foods).Contains(codeWater)) {
                    return false;
                }
            }
            return true;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (_project.UncertaintyAnalysisSettings.ReSampleConcentrations) {
                result.Add(UncertaintySource.Concentrations);
                if (_project.ConcentrationModelSettings.UseComplexResidueDefinitions) {
                    result.Add(UncertaintySource.ActiveSubstanceAllocation);
                }
                if (_project.ConcentrationModelSettings.IsFocalCommodityMeasurementReplacement) {
                    result.Add(UncertaintySource.FocalCommodityReplacement);
                }
                if (_project.ConcentrationModelSettings.ExtrapolateConcentrations) {
                    result.Add(UncertaintySource.ConcentrationExtrapolation);
                }
            }
            return result;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new ConcentrationsSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ConcentrationsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var settings = new ConcentrationsModuleSettings(_project);

            // Load the food samples
            var foodSamples = subsetManager.SelectedFoodSamples;

            // Filter by analysed substances
            var analysedSubstancesFilter = new AnalysedSubstancesFoodSampleFilter(data.AllCompounds);
            foodSamples = foodSamples.Where(r => analysedSubstancesFilter.Passes(r)).ToList();

            // Check if there are any food samples left
            if (!foodSamples.Any()) {
                throw new Exception("No food samples selected.");
            }

            // Filter by sample property
            if (settings.IsSamplePropertySubset) {

                var population = data.SelectedPopulation;

                // Check for sample period subset
                if (settings?.PeriodSubsetDefinition.AlignSampleDateSubsetWithPopulation ?? false) {
                    // Subset based on population
                    if (population?.StartDate != null && population?.EndDate != null) {
                        var filter = new SamplePeriodFilter(
                            new List<TimeRange>() { new TimeRange(population.StartDate.Value, population.EndDate.Value) },
                            settings.PeriodSubsetDefinition.IncludeMissingValueRecords
                        );
                        foodSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
                    }
                } else if (settings.PeriodSubsetDefinition?.YearsSubset?.Any() ?? false) {
                    // Subset based selected years
                    var filter = new SamplePeriodFilter(
                        settings.PeriodSubsetDefinition.YearsSubsetTimeRanges,
                        settings.PeriodSubsetDefinition.IncludeMissingValueRecords
                    );
                    foodSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
                }

                // Sample season (months) subset
                if (settings?.PeriodSubsetDefinition?.AlignSampleDateSubsetWithPopulation ?? false) {
                    // Months subset from population
                    if (population != null && population.PopulationIndividualPropertyValues.TryGetValue("Month", out var monthLevels)) {
                        var months = monthLevels.CategoricalLevels.Select(r => (int)MonthTypeConverter.FromString(r)).ToList();
                        var filter = new SampleMonthsFilter(
                            months,
                            settings.PeriodSubsetDefinition.IncludeMissingValueRecords
                        );
                        foodSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
                    }
                } else if (settings.PeriodSubsetDefinition?.MonthsSubset?.Any() ?? false) {
                    // Months subset from settings
                    var filter = new SampleMonthsFilter(
                        settings.PeriodSubsetDefinition.MonthsSubset,
                        settings.PeriodSubsetDefinition.IncludeMissingValueRecords
                    );
                    foodSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
                }

                // Location subset
                if (settings.LocationSubsetDefinition?.AlignSubsetWithPopulation ?? false) {
                    if (!string.IsNullOrEmpty(population?.Location)) {
                        // Location subset from population
                        var filter = new SampleLocationFilter(
                            settings.LocationSubsetDefinition.LocationSubset,
                            settings.LocationSubsetDefinition.IncludeMissingValueRecords
                        );
                        foodSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
                    }
                } else if (settings.LocationSubsetDefinition?.LocationSubset?.Any() ?? false) {
                    // Location subset from settings
                    var filter = new SampleLocationFilter(
                        settings.LocationSubsetDefinition.LocationSubset,
                        settings.LocationSubsetDefinition.IncludeMissingValueRecords
                    );
                    foodSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
                }

                // Region subset
                if (settings.RegionSubsetDefinition?.AlignSubsetWithPopulation ?? false) {
                    // Region subset from population
                    if (population?.PopulationIndividualPropertyValues != null &&
                        population.PopulationIndividualPropertyValues.TryGetValue("Region", out var region)
                    ) {
                        var regionSubsetDefinition = settings.RegionSubsetDefinition;
                        var filter = new SamplePropertyFilter(
                            regionSubsetDefinition.PropertyName,
                            region.CategoricalLevels,
                            (foodSample) => foodSample.Region,
                            regionSubsetDefinition.IncludeMissingValueRecords
                        );
                        foodSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
                    }
                } else if (settings.RegionSubsetDefinition != null) {
                    // Region subset from settings
                    var filter = new SamplePropertyFilter(
                        settings.RegionSubsetDefinition.PropertyName,
                        settings.RegionSubsetDefinition.KeyWords,
                        (foodSample) => foodSample.Region,
                        settings.RegionSubsetDefinition.IncludeMissingValueRecords
                    );
                    foodSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
                }

                // Check for production method subsets
                if (settings.ProductionMethodSubsetDefinition != null) {
                    var filter = new SamplePropertyFilter(
                        settings.ProductionMethodSubsetDefinition.PropertyName,
                        settings.ProductionMethodSubsetDefinition.KeyWords,
                        (foodSample) => foodSample.ProductionMethod,
                        settings.ProductionMethodSubsetDefinition.IncludeMissingValueRecords
                    );
                    foodSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
                }

                // Check for additional property subsets
                if (settings.AdditionalSamplePropertySubsetDefinitions?.Any() ?? false) {
                    foreach (var additionalPropertySubset in settings.AdditionalSamplePropertySubsetDefinitions) {
                        if (subsetManager.AllAdditionalSampleProperties.TryGetValue(additionalPropertySubset.PropertyName, out var property)) {
                            var filter = new SamplePropertyFilter(
                                additionalPropertySubset.PropertyName,
                                additionalPropertySubset.KeyWords,
                                (foodSample) => foodSample.SampleProperties.TryGetValue(property, out var value) ? value.TextValue : null,
                                additionalPropertySubset.IncludeMissingValueRecords
                            );
                            foodSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
                        } else {
                            throw new Exception($"Additional sample property subset defined on property '{additionalPropertySubset.PropertyName}', which is not available in the samples.");
                        }
                    }
                }

                // Check if there are any samples left
                if (!foodSamples.Any()) {
                    throw new Exception("No food samples remaining after sample subset selection.");
                }
            }

            data.FoodSamples = foodSamples.ToLookup(r => r.Food);

            // Apply MRL exceedance filter
            if (settings.FilterConcentrationLimitExceedingSamples && data.MaximumConcentrationLimits != null) {
                var filter = new MrlExceedanceSamplesFilter(data.MaximumConcentrationLimits.Values, settings.ConcentrationLimitFilterFractionExceedanceThreshold);
                foodSamples = foodSamples.Where(r => filter.Passes(r)).ToList();
            }

            // Filter by focal-food / non-focal-food
            if (settings.FocalCommodity && data.FocalCommoditySamples != null) {
                var focalCommodityFoodCodes = _project.FocalFoods?
                    .Select(r => r.CodeFood)
                    .Distinct()
                    .Where(r => !string.IsNullOrEmpty(r) && data.AllFoodsByCode.ContainsKey(r));
                var focalCommodityFoods = focalCommodityFoodCodes.Select(r => data.AllFoodsByCode[r]).ToHashSet();
                if (settings.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSamples) {
                    foodSamples = foodSamples
                        .Where(s => !focalCommodityFoods.Contains(s.Food))
                        .Union(data.FocalCommoditySamples)
                        .ToList();
                } else if (settings.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.AppendSamples) {
                    foodSamples = foodSamples
                        .Union(data.FocalCommoditySamples)
                        .ToList();
                }
            }

            // Find the measured foods
            data.ModelledFoods = foodSamples.Select(r => r.Food).ToHashSet();
            if (settings.ExtrapolateConcentrations && data.FoodExtrapolations != null) {
                data.ModelledFoods = data.FoodExtrapolations.Keys.Union(data.ModelledFoods).ToHashSet();
            }

            // Find the measured substances
            if (settings.UseComplexResidueDefinitions) {
                data.MeasuredSubstances =
                    foodSamples
                        .SelectMany(c => c.SampleAnalyses)
                        .SelectMany(s => s.AnalyticalMethod != null ? s.AnalyticalMethod.AnalyticalMethodCompounds.Keys : s.Concentrations.Keys)
                        .ToHashSet();
            } else {
                data.MeasuredSubstances = data.AllCompounds
                        .Where(c => foodSamples.SelectMany(r => r.SampleAnalyses)
                        .Any(r => (r.AnalyticalMethod != null && r.AnalyticalMethod.AnalyticalMethodCompounds.ContainsKey(c))
                            || r.AnalyticalMethod == null && r.Concentrations.ContainsKey(c)))
                        .ToHashSet();
            }

            // Compute sample origins
            data.SampleOriginInfos = SampleOriginCalculator.Calculate(foodSamples.ToLookup(c => c.Food));

            // For focal commodity substance measurement removal/replacement compute the focal commodity combinations.
            if (settings.FocalCommodity && settings.IsFocalCommodityMeasurementReplacement()) {
                data.FocalCommodityCombinations = FocalCommodityCombinationsBuilder
                    .Create(
                        settings.FocalFoods,
                        data.AllFoodsByCode,
                        data.AllCompounds.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase)
                    )
                    .ToList();

                // Make sure that the focal food/substance combinations are authorised.
                // Note: this implementation is not so nice; the concentrations module is now changing
                // data of another module (the substance authorisations). Candidate for improvement.
                if (data.SubstanceAuthorisations != null) {
                    var unauthorisedCombinations = data.FocalCommodityCombinations
                        .Where(r => !data.SubstanceAuthorisations.ContainsKey((r.Food, r.Substance)))
                        .ToList();
                    foreach (var combination in unauthorisedCombinations) {
                        data.SubstanceAuthorisations.Add(
                            (combination.Food, combination.Substance),
                            new SubstanceAuthorisation() {
                                Food = combination.Food,
                                Substance = combination.Substance
                            }
                        );
                    }
                }
            }

            // Compute substance sample collections
            data.MeasuredSubstanceSampleCollections = SampleCompoundCollectionsBuilder.Create(
                data.ModelledFoods,
                data.MeasuredSubstances,
                foodSamples,
                data.ConcentrationUnit,
                data.SubstanceAuthorisations,
                progressState
            );

            // Main random generator
            var mainRandomGenerator = GetRandomGenerator(_project.MonteCarloSettings.RandomSeed);

            // Random generator for allocation of active substances
            var allocationRandomGenerator = Simulation.IsBackwardCompatibilityMode
                ? mainRandomGenerator
                : new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.CONC_RandomActiveSubstanceAllocation));

            // Random generator for extrapolation
            var extrapolationRandomGenerator = Simulation.IsBackwardCompatibilityMode
                ? mainRandomGenerator
                : new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.CONC_RandomConcentrationExtrapolation));

            // Random generator for focal commodity concentration replacement
            var focalCommodityReplacementRandomGenerator = Simulation.IsBackwardCompatibilityMode
                ? GetRandomGenerator(_project.MonteCarloSettings.RandomSeed)
                : new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.CONC_RandomFocalConcentrationReplacement));

            compute(
                data,
                allocationRandomGenerator,
                extrapolationRandomGenerator,
                focalCommodityReplacementRandomGenerator,
                progressState
            );
        }

        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            if (_project.UncertaintyAnalysisSettings.ReSampleConcentrations) {

                var settings = new ConcentrationsModuleSettings(_project);

                if (factorialSet.Contains(UncertaintySource.Concentrations)
                    || factorialSet.Contains(UncertaintySource.ActiveSubstanceAllocation)
                    || factorialSet.Contains(UncertaintySource.ConcentrationExtrapolation)
                    || factorialSet.Contains(UncertaintySource.FocalCommodityReplacement)
                ) {

                    if (factorialSet.Contains(UncertaintySource.Concentrations)) { 
                        // Bootstrap measured substance sample compound collections
                        var newMeasuredSubstanceSampleCollections = SampleCompoundCollectionsBuilder
                            .ResampleSampleCompoundCollections(
                                data.MeasuredSubstanceSampleCollections,
                                Simulation.IsBackwardCompatibilityMode
                                    ? uncertaintySourceGenerators[UncertaintySource.Concentrations].Next(1, int.MaxValue)
                                    : uncertaintySourceGenerators[UncertaintySource.Concentrations].Seed
                            );
                        data.MeasuredSubstanceSampleCollections = newMeasuredSubstanceSampleCollections;
                    }

                    // Focal commodity random generator
                    var focalCommodityReplacementRandomGenerator = Simulation.IsBackwardCompatibilityMode
                        ? GetRandomGenerator(_project.MonteCarloSettings.RandomSeed)
                        : settings.IsFocalCommodityMeasurementReplacement()
                            ? uncertaintySourceGenerators[UncertaintySource.FocalCommodityReplacement]
                            : null;

                    // Active substance allocation random generator
                    var allocationRandomGenerator = Simulation.IsBackwardCompatibilityMode
                        ? uncertaintySourceGenerators[UncertaintySource.Concentrations]
                        : settings.UseComplexResidueDefinitions
                            ? uncertaintySourceGenerators[UncertaintySource.ActiveSubstanceAllocation]
                            : null;

                    // Extrapolation random random generator
                    var extrapolationRandomGenerator = Simulation.IsBackwardCompatibilityMode
                        ? uncertaintySourceGenerators[UncertaintySource.Concentrations]
                        : settings.ExtrapolateConcentrations
                            ? uncertaintySourceGenerators[UncertaintySource.ConcentrationExtrapolation]
                            : null;

                    compute(
                        data,
                        allocationRandomGenerator,
                        extrapolationRandomGenerator,
                        focalCommodityReplacementRandomGenerator
                    );
                }
            }
        }

        private void compute(
            ActionData data,
            IRandom allocationRandomGenerator,
            IRandom extrapolationRandomGenerator,
            IRandom focalCommodityReplacementRandomGenerator,
            CompositeProgressState progressState = null
        ) {
            var settings = new ConcentrationsModuleSettings(_project);

            var measuredSubstanceSampleCollections = data.MeasuredSubstanceSampleCollections
                .Select(r => r.Clone()).ToList();

            // Focal commodity substance measurement removal/replacement
            if (settings.FocalCommodity
                && settings.IsFocalCommodityMeasurementReplacement()
                && !settings.UseDeterministicSubstanceConversionsForFocalCommodity
            ) {
                var calculator = new FocalCommodityMeasurementReplacementCalculatorFactory(settings);
                var focalCommodityReplacementCalculator = calculator.Create(
                        data.FocalCommoditySubstanceSampleCollections,
                        data.MaximumConcentrationLimits,
                        null,
                        data.ConcentrationUnit
                    );

                measuredSubstanceSampleCollections = focalCommodityReplacementCalculator
                    .Compute(
                        measuredSubstanceSampleCollections?.ToDictionary(r => r.Food),
                        data.FocalCommodityCombinations,
                        focalCommodityReplacementRandomGenerator
                    ).Values
                    .ToList();
            }

            // Compute active substance sample compound collections
            if (settings.UseComplexResidueDefinitions) {
                var activeSubstanceAllocationCalculatorFactory = new ActiveSubstanceAllocationCalculatorFactory(settings);
                var calculator = activeSubstanceAllocationCalculatorFactory.Create(
                    data.SubstanceConversions,
                    data.SubstanceAuthorisations,
                    data.CorrectedRelativePotencyFactors
                );
                var activeSubstanceSampleCollections = calculator.Allocate(
                    measuredSubstanceSampleCollections,
                    new HashSet<Compound>(data.ActiveSubstances),
                    allocationRandomGenerator,
                    progressState
                );
                data.ActiveSubstanceSampleCollections = activeSubstanceSampleCollections;
            } else {
                data.ActiveSubstanceSampleCollections = measuredSubstanceSampleCollections;
            }

            // Compute food extrapolation candidates
            if (settings.ExtrapolateConcentrations && data.FoodExtrapolations != null) {
                var foods = data.ModelledFoods.Union(data.FoodExtrapolations.Keys).ToList();
                var foodExtrapolationCandidatesCalculator = new FoodExtrapolationCandidatesCalculator(settings);
                var extrapolationCandidates = foodExtrapolationCandidatesCalculator.ComputeExtrapolationCandidates(
                    foods,
                    data.ActiveSubstances ?? data.AllCompounds,
                    data.ActiveSubstanceSampleCollections.ToDictionary(r => r.Food),
                    data.FoodExtrapolations,
                    data.SubstanceConversions,
                    data.SubstanceAuthorisations,
                    data.MaximumConcentrationLimits
                );
                data.ExtrapolationCandidates = extrapolationCandidates;
                MissingValueExtrapolationCalculator.ExtrapolateMissingValues(
                    data.ActiveSubstanceSampleCollections.ToDictionary(r => r.Food),
                    data.ExtrapolationCandidates,
                    extrapolationRandomGenerator
                );
            }

            // Extrapolation of water
            if (settings.ImputeWaterConcentrations
                && data.AllFoodsByCode.TryGetValue(settings.CodeWater, out var water)
            ) {
                var waterImputationCalculator = new WaterConcentrationsExtrapolationCalculator(settings);

                if (data.ActiveSubstanceSampleCollections.Any(r => r.Food == water)) {
                    throw new Exception($"Unexpected: found concentration data for {water.Name}, imputation not possible");
                }
                var waterSampleCollection = waterImputationCalculator.Create(
                    data.ActiveSubstances ?? data.AllCompounds,
                    water,
                    data.SubstanceAuthorisations,
                    5,
                    data.CorrectedRelativePotencyFactors,
                    data.ConcentrationUnit
                );
                data.ActiveSubstanceSampleCollections.Add(waterSampleCollection);
                data.ModelledFoods.Add(water);
            }

            if (_project.AssessmentSettings.FocalCommodity
                && settings.IsFocalCommodityMeasurementReplacement()
                && settings.UseDeterministicSubstanceConversionsForFocalCommodity
            ) {
                var focalCommodityCalculator = new FocalCommodityMeasurementReplacementCalculatorFactory(settings);
                var focalCommodityReplacementCalculator = focalCommodityCalculator
                    .Create(
                        data.FocalCommoditySubstanceSampleCollections,
                        data.MaximumConcentrationLimits,
                        data.DeterministicSubstanceConversionFactors,
                        data.ConcentrationUnit
                    );

                data.ActiveSubstanceSampleCollections = focalCommodityReplacementCalculator
                    .Compute(
                        data.ActiveSubstanceSampleCollections?.ToDictionary(r => r.Food),
                        data.FocalCommodityCombinations,
                        focalCommodityReplacementRandomGenerator
                    ).Values
                    .ToList();
            }
        }

        protected override void summarizeActionResult(IConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ConcentrationsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
