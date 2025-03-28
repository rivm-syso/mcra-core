﻿using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ActiveSubstanceAllocation;
using MCRA.Simulation.Calculators.FocalCommodityCombinationsBuilder;
using MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation;
using MCRA.Simulation.Calculators.FoodExtrapolationsCalculation;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Calculators.SampleCompoundCollections;
using MCRA.Simulation.Calculators.SampleOriginCalculation;
using MCRA.Simulation.Filters.FoodSampleFilters;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.DateTimes;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.Concentrations {

    [ActionType(ActionType.Concentrations)]
    public sealed class ConcentrationsActionCalculator : ActionCalculatorBase<IConcentrationsActionResult> {
        private ConcentrationsModuleConfig ModuleConfig => (ConcentrationsModuleConfig)_moduleSettings;

        public ConcentrationsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var requireFocalFoodConcentrations = ModuleConfig.FocalCommodity
                && (ModuleConfig.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstances
                    || ModuleConfig.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSamples
                    || ModuleConfig.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.AppendSamples
                );
            _actionInputRequirements[ActionType.FocalFoodConcentrations].IsRequired = requireFocalFoodConcentrations;
            _actionInputRequirements[ActionType.FocalFoodConcentrations].IsVisible = requireFocalFoodConcentrations;

            var regionSubsetDefinition = ModuleConfig.SamplesSubsetDefinitions?.FirstOrDefault(r => r.IsRegionSubset);
            var alignSampleSubsetWithPopulation = ModuleConfig.SampleSubsetSelection
                && ((ModuleConfig.PeriodSubsetDefinition?.AlignSampleDateSubsetWithPopulation ?? false)
                    || (ModuleConfig.PeriodSubsetDefinition?.AlignSampleSeasonSubsetWithPopulation ?? false)
                    || (ModuleConfig.LocationSubsetDefinition?.AlignSubsetWithPopulation ?? false)
                    || (regionSubsetDefinition?.AlignSubsetWithPopulation ?? false)
                );
            _actionInputRequirements[ActionType.Populations].IsRequired = alignSampleSubsetWithPopulation;
            _actionInputRequirements[ActionType.Populations].IsVisible = alignSampleSubsetWithPopulation;

            var useComplexResidueDefinitions = ModuleConfig.UseComplexResidueDefinitions;
            _actionInputRequirements[ActionType.SubstanceConversions].IsVisible = useComplexResidueDefinitions;
            _actionInputRequirements[ActionType.SubstanceConversions].IsRequired = useComplexResidueDefinitions;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = useComplexResidueDefinitions;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = useComplexResidueDefinitions;
            var isExtrapolation = ModuleConfig.ExtrapolateConcentrations;
            _actionInputRequirements[ActionType.FoodExtrapolations].IsRequired = isExtrapolation;
            _actionInputRequirements[ActionType.FoodExtrapolations].IsVisible = isExtrapolation;
            var isMrlInput = ModuleConfig.FilterConcentrationLimitExceedingSamples
                || (isExtrapolation && ModuleConfig.ConsiderMrlForExtrapolations)
                || (ModuleConfig.FocalCommodity && ModuleConfig.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue);
            _actionInputRequirements[ActionType.ConcentrationLimits].IsRequired = isMrlInput;
            _actionInputRequirements[ActionType.ConcentrationLimits].IsVisible = isMrlInput;
            var isWaterImputation = ModuleConfig.ImputeWaterConcentrations;
            var substanceAuthorisations = (isExtrapolation && ModuleConfig.ConsiderAuthorisationsForExtrapolations)
                || (useComplexResidueDefinitions && ModuleConfig.ConsiderAuthorisationsForSubstanceConversion)
                || (isWaterImputation && ModuleConfig.RestrictWaterImputationToAuthorisedUses);
            _actionInputRequirements[ActionType.SubstanceAuthorisations].IsRequired = substanceAuthorisations;
            _actionInputRequirements[ActionType.SubstanceAuthorisations].IsVisible = substanceAuthorisations;
            var isWaterImputationWithPotencyRestrition = isWaterImputation
                && ModuleConfig.RestrictWaterImputationToMostPotentSubstances;
            var isRestrictWaterImputationToApprovedSubstances = isWaterImputation
               && ModuleConfig.RestrictWaterImputationToApprovedSubstances;
            _actionInputRequirements[ActionType.SubstanceApprovals].IsRequired = isRestrictWaterImputationToApprovedSubstances;
            _actionInputRequirements[ActionType.SubstanceApprovals].IsVisible = isRestrictWaterImputationToApprovedSubstances;
            var rpfVisible = isWaterImputationWithPotencyRestrition ||
                (useComplexResidueDefinitions &&
                ModuleConfig.SubstanceTranslationAllocationMethod == SubstanceTranslationAllocationMethod.UseMostToxic);
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = rpfVisible;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = rpfVisible;

            var isFocalCommodityMeasurementReplacement = ModuleConfig.FocalCommodity
                && (ModuleConfig.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstances
                    || ModuleConfig.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue
                    || ModuleConfig.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByProposedLimitValue
                );
            var useDeterministicSubstanceConversions = isFocalCommodityMeasurementReplacement
                && ModuleConfig.UseDeterministicSubstanceConversionsForFocalCommodity;
            _actionInputRequirements[ActionType.DeterministicSubstanceConversionFactors].IsVisible = useDeterministicSubstanceConversions;
            _actionInputRequirements[ActionType.DeterministicSubstanceConversionFactors].IsRequired = useDeterministicSubstanceConversions;

            var useProcessingFactors = isFocalCommodityMeasurementReplacement
                && ModuleConfig.FocalCommodityIncludeProcessedDerivatives;
            _actionInputRequirements[ActionType.ProcessingFactors].IsVisible = useProcessingFactors;
            _actionInputRequirements[ActionType.ProcessingFactors].IsRequired = useProcessingFactors;

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
            if (ModuleConfig.ImputeWaterConcentrations) {
                var codeWater = ModuleConfig.CodeWater;
                if (!linkManager.GetCodesInScope(ScopingType.Foods).Contains(codeWater)) {
                    return false;
                }
            }
            return true;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleConcentrations) {
                result.Add(UncertaintySource.Concentrations);
                if (ModuleConfig.UseComplexResidueDefinitions) {
                    result.Add(UncertaintySource.ActiveSubstanceAllocation);
                }
                if (ModuleConfig.IsFocalCommodityMeasurementReplacement) {
                    result.Add(UncertaintySource.FocalCommodityReplacement);
                }
                if (ModuleConfig.ExtrapolateConcentrations) {
                    result.Add(UncertaintySource.ConcentrationExtrapolation);
                }
            }
            return result;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new ConcentrationsSettingsManager();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new ConcentrationsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            // Load the food samples
            var foodSamples = subsetManager.SelectedFoodSamples;

            // Filter by analysed substances
            var analysedSubstancesFilter = new AnalysedSubstancesFoodSampleFilter(data.AllCompounds);
            foodSamples = foodSamples.Where(analysedSubstancesFilter.Passes).ToList();

            // Check if there are any food samples left
            if (!foodSamples.Any()) {
                throw new Exception("No food samples selected.");
            }

            // Filter by sample property
            if (ModuleConfig.IsSamplePropertySubset) {

                var population = data.SelectedPopulation;

                // Check for sample period subset
                if (ModuleConfig?.PeriodSubsetDefinition?.AlignSampleDateSubsetWithPopulation ?? false) {
                    // Subset based on population
                    if (population?.StartDate != null && population?.EndDate != null) {
                        var filter = new SamplePeriodFilter(
                            [new TimeRange(population.StartDate.Value, population.EndDate.Value)],
                            ModuleConfig.PeriodSubsetDefinition.IncludeMissingValueRecords
                        );
                        foodSamples = foodSamples.Where(filter.Passes).ToList();
                    }
                } else if (ModuleConfig.PeriodSubsetDefinition?.YearsSubset?.Count > 0) {
                    // Subset based selected years
                    var filter = new SamplePeriodFilter(
                        ModuleConfig.PeriodSubsetDefinition.YearsSubsetTimeRanges,
                        ModuleConfig.PeriodSubsetDefinition.IncludeMissingValueRecords
                    );
                    foodSamples = foodSamples.Where(filter.Passes).ToList();
                }

                // Sample season (months) subset
                if (ModuleConfig?.PeriodSubsetDefinition?.AlignSampleDateSubsetWithPopulation ?? false) {
                    // Months subset from population
                    if (population != null && population.PopulationIndividualPropertyValues.TryGetValue("Month", out var monthLevels)) {
                        var months = monthLevels.CategoricalLevels.Select(r => (int)MonthTypeConverter.FromString(r)).ToList();
                        var filter = new SampleMonthsFilter(
                            months,
                            ModuleConfig.PeriodSubsetDefinition.IncludeMissingValueRecords
                        );
                        foodSamples = foodSamples.Where(filter.Passes).ToList();
                    }
                } else if (ModuleConfig.PeriodSubsetDefinition?.MonthsSubset?.Count > 0) {
                    // Months subset from settings
                    var filter = new SampleMonthsFilter(
                        ModuleConfig.PeriodSubsetDefinition.MonthsSubset,
                        ModuleConfig.PeriodSubsetDefinition.IncludeMissingValueRecords
                    );
                    foodSamples = foodSamples.Where(filter.Passes).ToList();
                }

                // Location subset
                if (ModuleConfig.LocationSubsetDefinition?.AlignSubsetWithPopulation ?? false) {
                    if (!string.IsNullOrEmpty(population?.Location)) {
                        // Location subset from population
                        var filter = new SampleLocationFilter(
                            ModuleConfig.LocationSubsetDefinition.LocationSubset,
                            ModuleConfig.LocationSubsetDefinition.IncludeMissingValueRecords
                        );
                        foodSamples = foodSamples.Where(filter.Passes).ToList();
                    }
                } else if (ModuleConfig.LocationSubsetDefinition?.LocationSubset?.Count > 0) {
                    // Location subset from settings
                    var filter = new SampleLocationFilter(
                        ModuleConfig.LocationSubsetDefinition.LocationSubset,
                        ModuleConfig.LocationSubsetDefinition.IncludeMissingValueRecords
                    );
                    foodSamples = foodSamples.Where(filter.Passes).ToList();
                }

                // Region subset
                if (ModuleConfig.RegionSubsetDefinition?.AlignSubsetWithPopulation ?? false) {
                    // Region subset from population
                    if (population?.PopulationIndividualPropertyValues != null &&
                        population.PopulationIndividualPropertyValues.TryGetValue("Region", out var region)
                    ) {
                        var regionSubsetDefinition = ModuleConfig.RegionSubsetDefinition;
                        var filter = new SamplePropertyFilter(
                            region.CategoricalLevels,
                            (foodSample) => foodSample.Region,
                            regionSubsetDefinition.IncludeMissingValueRecords
                        );
                        foodSamples = foodSamples.Where(filter.Passes).ToList();
                    }
                } else if (ModuleConfig.RegionSubsetDefinition != null) {
                    // Region subset from settings
                    var filter = new SamplePropertyFilter(
                        ModuleConfig.RegionSubsetDefinition.KeyWords,
                        (foodSample) => foodSample.Region,
                        ModuleConfig.RegionSubsetDefinition.IncludeMissingValueRecords
                    );
                    foodSamples = foodSamples.Where(filter.Passes).ToList();
                }

                // Check for production method subsets
                if (ModuleConfig.ProductionMethodSubsetDefinition != null) {
                    var filter = new SamplePropertyFilter(
                        ModuleConfig.ProductionMethodSubsetDefinition.KeyWords,
                        (foodSample) => foodSample.ProductionMethod,
                        ModuleConfig.ProductionMethodSubsetDefinition.IncludeMissingValueRecords
                    );
                    foodSamples = foodSamples.Where(filter.Passes).ToList();
                }

                // Check for additional property subsets
                if (ModuleConfig.AdditionalSamplePropertySubsetDefinitions?.Count > 0) {
                    foreach (var additionalPropertySubset in ModuleConfig.AdditionalSamplePropertySubsetDefinitions) {
                        if (subsetManager.AllAdditionalSampleProperties.TryGetValue(additionalPropertySubset.PropertyName, out var property)) {
                            var filter = new SamplePropertyFilter(
                                additionalPropertySubset.KeyWords,
                                (foodSample) => foodSample.SampleProperties.TryGetValue(property, out var value) ? value.TextValue : null,
                                additionalPropertySubset.IncludeMissingValueRecords
                            );
                            foodSamples = foodSamples.Where(filter.Passes).ToList();
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
            if (ModuleConfig.FilterConcentrationLimitExceedingSamples && data.MaximumConcentrationLimits != null) {
                var filter = new MrlExceedanceSamplesFilter(data.MaximumConcentrationLimits.Values, ModuleConfig.ConcentrationLimitFilterFractionExceedanceThreshold);
                foodSamples = foodSamples.Where(filter.Passes).ToList();
            }

            // Filter by focal-food / non-focal-food
            if (ModuleConfig.FocalCommodity && data.FocalCommoditySamples != null) {
                var focalCommodityFoodCodes = this.ModuleConfig.FocalFoods?
                    .Select(r => r.CodeFood)
                    .Distinct()
                    .Where(r => !string.IsNullOrEmpty(r) && data.AllFoodsByCode.ContainsKey(r));
                var focalCommodityFoods = focalCommodityFoodCodes.Select(r => data.AllFoodsByCode[r]).ToHashSet();
                if (ModuleConfig.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSamples) {
                    foodSamples = foodSamples
                        .Where(s => !focalCommodityFoods.Contains(s.Food))
                        .Union(data.FocalCommoditySamples)
                        .ToList();
                } else if (ModuleConfig.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.AppendSamples) {
                    foodSamples = foodSamples
                        .Union(data.FocalCommoditySamples)
                        .ToList();
                }
                if (ModuleConfig.FilterProcessedFocalCommoditySamples) {
                    foodSamples = foodSamples
                    .Where(c => !(c.Food.BaseFood != null && focalCommodityFoodCodes.Contains(c.Food.BaseFood.Code)))
                    .ToList();
                }
            }

            // Find the measured foods
            data.MeasuredFoods = foodSamples.Select(r => r.Food).ToHashSet();
            if (ModuleConfig.ExtrapolateConcentrations && data.FoodExtrapolations != null) {
                data.MeasuredFoods = data.FoodExtrapolations.Keys.Union(data.MeasuredFoods).ToHashSet();
            }

            // Find the measured substances
            if (ModuleConfig.UseComplexResidueDefinitions) {
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
            if (ModuleConfig.FocalCommodity && ModuleConfig.IsFocalCommodityMeasurementReplacement) {
                data.FocalCommodityCombinations = FocalCommodityCombinationsBuilder
                    .Create(
                        ModuleConfig.FocalFoods,
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
            //Set concentration unit
            data.ConcentrationUnit = data.MeasuredSubstances.Count == 1
                ? data.MeasuredSubstances.First().ConcentrationUnit
                : ConcentrationUnit.mgPerKg;

            // Compute substance sample collections
            data.MeasuredSubstanceSampleCollections = SampleCompoundCollectionsBuilder
                .Create(
                    data.MeasuredFoods,
                    data.MeasuredSubstances,
                    foodSamples,
                    data.ConcentrationUnit,
                    data.SubstanceAuthorisations,
                    progressState
                );

            // Main random generator
            var mainRandomGenerator = GetRandomGenerator(this.ModuleConfig.RandomSeed);

            // Random generator for allocation of active substances
            var allocationRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(this.ModuleConfig.RandomSeed, (int)RandomSource.CONC_RandomActiveSubstanceAllocation));

            // Random generator for extrapolation
            var extrapolationRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(this.ModuleConfig.RandomSeed, (int)RandomSource.CONC_RandomConcentrationExtrapolation));

            // Random generator for focal commodity concentration replacement
            var focalCommodityReplacementRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(this.ModuleConfig.RandomSeed, (int)RandomSource.CONC_RandomFocalConcentrationReplacement));

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
            var localProgress = progressReport.NewProgressState(100);
            if (ModuleConfig.ResampleConcentrations) {
                if (factorialSet.Contains(UncertaintySource.Concentrations)
                    || factorialSet.Contains(UncertaintySource.ActiveSubstanceAllocation)
                    || factorialSet.Contains(UncertaintySource.ConcentrationExtrapolation)
                    || factorialSet.Contains(UncertaintySource.FocalCommodityReplacement)
                ) {
                    if (factorialSet.Contains(UncertaintySource.Concentrations)) {
                        // Bootstrap measured substance sample compound collections
                        var newMeasuredSubstanceSampleCollections = SampleCompoundCollectionsBuilder
                            .ResampleSampleCompoundCollections(
                                data.MeasuredSubstanceSampleCollections.Values,
                                uncertaintySourceGenerators[UncertaintySource.Concentrations].Seed
                            );
                        data.MeasuredSubstanceSampleCollections = newMeasuredSubstanceSampleCollections
                            .ToDictionary(r => r.Food);
                    }

                    // Focal commodity random generator
                    var focalCommodityReplacementRandomGenerator = ModuleConfig.IsFocalCommodityMeasurementReplacement
                        ? uncertaintySourceGenerators[UncertaintySource.FocalCommodityReplacement]
                        : null;

                    // Active substance allocation random generator
                    var allocationRandomGenerator = ModuleConfig.UseComplexResidueDefinitions
                        ? uncertaintySourceGenerators[UncertaintySource.ActiveSubstanceAllocation]
                        : null;

                    // Extrapolation random random generator
                    var extrapolationRandomGenerator = ModuleConfig.ExtrapolateConcentrations
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
            localProgress.Update(100);
        }

        private void compute(
            ActionData data,
            IRandom allocationRandomGenerator,
            IRandom extrapolationRandomGenerator,
            IRandom focalCommodityReplacementRandomGenerator,
            CompositeProgressState progressState = null
        ) {
            var measuredSubstanceSampleCollections = data.MeasuredSubstanceSampleCollections.Values
                .ToDictionary(r => r.Food, r => r.Clone());

            // Focal commodity substance measurement removal/replacement
            if (ModuleConfig.FocalCommodity
                && ModuleConfig.IsFocalCommodityMeasurementReplacement
                && !ModuleConfig.UseDeterministicSubstanceConversionsForFocalCommodity
            ) {
                var calculator = new FocalCommodityMeasurementReplacementCalculatorFactory(ModuleConfig);
                var focalCommodityReplacementCalculator = calculator.Create(
                        data.FocalCommoditySubstanceSampleCollections,
                        getMaximumConcentrationLimits(data, ModuleConfig),
                        null,
                        null,
                        data.ConcentrationUnit
                    );

                measuredSubstanceSampleCollections = focalCommodityReplacementCalculator
                    .Compute(
                        measuredSubstanceSampleCollections,
                        data.FocalCommodityCombinations,
                        focalCommodityReplacementRandomGenerator
                    );
            }

            // Compute active substance sample compound collections
            if (ModuleConfig.UseComplexResidueDefinitions) {
                var activeSubstanceAllocationCalculatorFactory = new ActiveSubstanceAllocationCalculatorFactory(ModuleConfig);
                var calculator = activeSubstanceAllocationCalculatorFactory.Create(
                    data.SubstanceConversions,
                    data.SubstanceAuthorisations,
                    data.CorrectedRelativePotencyFactors
                );
                var activeSubstanceSampleCollections = calculator
                    .Allocate(
                        measuredSubstanceSampleCollections.Values,
                        new HashSet<Compound>(data.ActiveSubstances),
                        allocationRandomGenerator,
                        progressState
                    )
                    .ToDictionary(r => r.Food);
                data.ActiveSubstanceSampleCollections = activeSubstanceSampleCollections;
                data.ModelledSubstances = data.ActiveSubstances;
            } else {
                data.ModelledSubstances = data.AllCompounds;
                data.ActiveSubstanceSampleCollections = measuredSubstanceSampleCollections;
            }

            // Compute food extrapolation candidates
            if (ModuleConfig.ExtrapolateConcentrations && data.FoodExtrapolations != null) {
                // TODO: the ordering of these foods below is needed for reproducibility
                // of the results. This should be resolved by making the missing value
                // extrapolation calculator random process independent of the ordering
                // of foods (issue #1518).
                var foods = data.MeasuredFoods
                    .Union(data.FoodExtrapolations.Keys)
                    .ToList();

                var foodExtrapolationCandidatesCalculator = new FoodExtrapolationCandidatesCalculator(ModuleConfig);
                var extrapolationCandidates = foodExtrapolationCandidatesCalculator
                    .ComputeExtrapolationCandidates(
                        foods,
                        data.ActiveSubstances ?? data.AllCompounds,
                        data.ActiveSubstanceSampleCollections,
                        data.FoodExtrapolations,
                        data.SubstanceConversions,
                        data.SubstanceAuthorisations,
                        data.MaximumConcentrationLimits
                    );
                data.ExtrapolationCandidates = extrapolationCandidates;
                MissingValueExtrapolationCalculator
                    .ExtrapolateMissingValues(
                        data.ActiveSubstanceSampleCollections,
                        data.ExtrapolationCandidates,
                        extrapolationRandomGenerator
                    );
            }

            // Extrapolation of water
            if (ModuleConfig.ImputeWaterConcentrations
                && data.AllFoodsByCode.TryGetValue(ModuleConfig.CodeWater, out var water)
            ) {
                var waterImputationCalculator = new WaterConcentrationsExtrapolationCalculator(ModuleConfig);
                if (data.ActiveSubstanceSampleCollections.ContainsKey(water)) {
                    throw new Exception($"Unexpected: found concentration data for {water.Name}, imputation not possible");
                }
                var waterSampleCollection = waterImputationCalculator.Create(
                    data.ActiveSubstances ?? data.AllCompounds,
                    water,
                    data.SubstanceAuthorisations,
                    data.SubstanceApprovals,
                    5,
                    data.CorrectedRelativePotencyFactors,
                    data.ConcentrationUnit
                );
                data.ActiveSubstanceSampleCollections.Add(water, waterSampleCollection);
            }

            if (this.ModuleConfig.FocalCommodity
                && ModuleConfig.IsFocalCommodityMeasurementReplacement
                && ModuleConfig.UseDeterministicSubstanceConversionsForFocalCommodity
            ) {
                var processingFactorProvider = ModuleConfig.FocalCommodityIncludeProcessedDerivatives
                    ? new ProcessingFactorProvider(data.ProcessingFactorModels, false, 1D) : null;

                var focalCommodityCalculator = new FocalCommodityMeasurementReplacementCalculatorFactory(ModuleConfig);
                var focalCommodityReplacementCalculator = focalCommodityCalculator
                    .Create(
                        data.FocalCommoditySubstanceSampleCollections,
                        getMaximumConcentrationLimits(data, ModuleConfig),
                        data.DeterministicSubstanceConversionFactors,
                        processingFactorProvider,
                        data.ConcentrationUnit
                    );

                data.ActiveSubstanceSampleCollections = focalCommodityReplacementCalculator
                    .Compute(
                        data.ActiveSubstanceSampleCollections,
                        data.FocalCommodityCombinations,
                        focalCommodityReplacementRandomGenerator
                    );
            }
        }
        protected override void summarizeActionResult(IConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ConcentrationsSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        /// <summary>
        /// Return Maximum Concentration Limits based on
        /// 1) a proposed concentration limit set in the interface
        /// 2) data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private IDictionary<(Food Food, Compound Substance), ConcentrationLimit> getMaximumConcentrationLimits(
            ActionData data,
            ConcentrationsModuleConfig settings
        ) {
            return settings.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByProposedLimitValue
                ? data.FocalCommodityCombinations
                    .ToDictionary(c => (c.Food, c.Substance), c => new ConcentrationLimit() {
                        Food = c.Food,
                        Compound = c.Substance,
                        ConcentrationUnit = ConcentrationUnit.mgPerKg,
                        Limit = settings.FocalCommodityProposedConcentrationLimit,
                        ValueType = ConcentrationLimitValueType.ProposedMaximumResidueLimit
                    })
                : data.MaximumConcentrationLimits;
        }
    }
}
