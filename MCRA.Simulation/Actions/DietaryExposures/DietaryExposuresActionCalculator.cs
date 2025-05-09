﻿using MCRA.Data.Management;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.ActionComparison;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.DietaryExposureImputationCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDayPruning;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IndividualDaysGenerator;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.IndividualAmountCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.ModelThenAddIntakeModelCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.OIMCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.UsualIntakeCalculation;
using MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation;
using MCRA.Simulation.Calculators.PopulationGeneration;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Calculators.TdsReductionFactorsCalculation;
using MCRA.Simulation.Calculators.UnitVariabilityCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.DietaryExposures {

    [ActionType(ActionType.DietaryExposures)]
    public class DietaryExposuresActionCalculator : ActionCalculatorBase<DietaryExposuresActionResult> {
        private DietaryExposuresModuleConfig ModuleConfig => (DietaryExposuresModuleConfig)_moduleSettings;

        public DietaryExposuresActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isMultipleSubstances = ModuleConfig.MultipleSubstances
                && !(IsLoopScope(ScopingType.Compounds));
            var isCumulative = isMultipleSubstances && ModuleConfig.Cumulative;
            var isRiskBasedMcr = ModuleConfig.MultipleSubstances
                && ModuleConfig.McrAnalysis
                && ModuleConfig.McrExposureApproachType == ExposureApproachType.RiskBased;
            var isTotalDietStudy = ModuleConfig.TotalDietStudy && ModuleConfig.ExposureType == ExposureType.Chronic;
            _actionInputRequirements[ActionType.Effects].IsRequired = isCumulative;
            _actionInputRequirements[ActionType.Effects].IsVisible = isCumulative;
            var useUnitVariabilityFactors = ModuleConfig.ExposureType == ExposureType.Acute && ModuleConfig.UseUnitVariability;
            _actionInputRequirements[ActionType.UnitVariabilityFactors].IsRequired = useUnitVariabilityFactors;
            _actionInputRequirements[ActionType.UnitVariabilityFactors].IsVisible = useUnitVariabilityFactors;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = isCumulative || isRiskBasedMcr;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = isCumulative || isRiskBasedMcr;
            var useProcessingFactors = !isTotalDietStudy && ModuleConfig.IsProcessing;
            _actionInputRequirements[ActionType.ProcessingFactors].IsRequired = useProcessingFactors;
            _actionInputRequirements[ActionType.ProcessingFactors].IsVisible = useProcessingFactors;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = isMultipleSubstances;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = isMultipleSubstances;
            var isCumulativeScreening = isCumulative && ModuleConfig.DietaryExposuresDetailsLevel == DietaryExposuresDetailsLevel.OnlyRiskDrivers;
            _actionInputRequirements[ActionType.HighExposureFoodSubstanceCombinations].IsRequired = isCumulativeScreening;
            _actionInputRequirements[ActionType.HighExposureFoodSubstanceCombinations].IsVisible = isCumulativeScreening;
            var useOccurrencePatterns = !ModuleConfig.IsSampleBased
                && ModuleConfig.UseOccurrencePatternsForResidueGeneration;
            _actionInputRequirements[ActionType.OccurrencePatterns].IsRequired = useOccurrencePatterns;
            _actionInputRequirements[ActionType.OccurrencePatterns].IsVisible = useOccurrencePatterns;

            var isTdsReductionToLimitScenario = ModuleConfig.TotalDietStudy && ModuleConfig.ReductionToLimitScenario;
            _actionInputRequirements[ActionType.FoodConversions].IsVisible = isTdsReductionToLimitScenario;
            _actionInputRequirements[ActionType.FoodConversions].IsRequired = isTdsReductionToLimitScenario;
            _actionInputRequirements[ActionType.ConcentrationDistributions].IsVisible = isTdsReductionToLimitScenario;
            _actionInputRequirements[ActionType.ConcentrationDistributions].IsRequired = isTdsReductionToLimitScenario;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new DietaryExposuresSettingsManager();
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResamplePortions) {
                result.Add(UncertaintySource.Portions);
            }
            if (ModuleConfig.ResampleImputationExposureDistributions) {
                result.Add(UncertaintySource.ImputeExposureDistributions);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new DietaryExposuresSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override DietaryExposuresActionResult run(ActionData data, CompositeProgressState progressReport) {
            var substances = data.ActiveSubstances;

            var localProgress = progressReport.NewProgressState(100);
            var result = new DietaryExposuresActionResult();
            result.DietaryExposureUnit = TargetUnit.CreateDietaryExposureUnit(
                data.ConsumptionUnit,
                data.ConcentrationUnit,
                data.BodyWeightUnit,
                ModuleConfig.IsPerPerson
            );

            // Create individual days
            localProgress.Update("Generating individual days", 30);
            var individualsRandomGenerator = new McraRandomGenerator(
                RandomUtils.CreateSeed(
                    ModuleConfig.RandomSeed,
                    (int)RandomSource.DE_DrawIndividuals
                )
            );

            var populationGeneratorFactory = new PopulationGeneratorFactory(
                ModuleConfig.ExposureType,
                ModuleConfig.IsSurveySampling,
                ModuleConfig.NumberOfMonteCarloIterations
            );
            var populationGenerator = populationGeneratorFactory.Create();

            var simulatedIndividualDays = populationGenerator.CreateSimulatedIndividualDays(
                data.ModelledFoodConsumers,
                data.ModelledFoodConsumerDays,
                individualsRandomGenerator
            );
            var simulatedIndividuals = simulatedIndividualDays
                .Select(s => s.SimulatedIndividual)
                .Distinct()
                .ToList();
            IndividualDaysGenerator.ImputeBodyWeight(simulatedIndividuals);

            result.SimulatedIndividualDays = simulatedIndividualDays;

            // Select only TDS compositions that are found in conversion algorithm
            if (ModuleConfig.ExposureType == ExposureType.Chronic && ModuleConfig.TotalDietStudy && ModuleConfig.ReductionToLimitScenario) {
                localProgress.Update("Computing TDS reduction factors", 33);
                var tdsReductionFactorsCalculator = new TdsReductionFactorsCalculator(data.ConcentrationDistributions);
                result.TdsReductionScenarioAnalysisFoods = ModuleConfig.ScenarioAnalysisFoods
                    .Where(r => data.AllFoodsByCode.ContainsKey(r))
                    .Select(r => data.AllFoodsByCode[r])
                    .ToList();
                result.TdsReductionFactors = tdsReductionFactorsCalculator
                    .CalculateReductionFactors(result.TdsReductionScenarioAnalysisFoods);
            }

            // Create residue generator
            var residueGeneratorFactory = new ResidueGeneratorFactory(
                ModuleConfig.UseOccurrencePatternsForResidueGeneration,
                ModuleConfig.SetMissingAgriculturalUseAsUnauthorized,
                ModuleConfig.IsSampleBased,
                ModuleConfig.DefaultConcentrationModel != ConcentrationModelType.Empirical,
                ModuleConfig.ExposureType,
                ModuleConfig.NonDetectsHandlingMethod
            );
            var residueGenerator = residueGeneratorFactory.Create(
                data.MonteCarloSubstanceSampleCollections?.ToDictionary(r => r.Food),
                data.ConcentrationModels,
                data.CumulativeConcentrationModels,
                data.CorrectedRelativePotencyFactors,
                data.MarginalOccurrencePatterns
            );

            //  Unit variability model calculator
            var unitVariabilityCalculator = ModuleConfig.ExposureType == ExposureType.Acute && ModuleConfig.UseUnitVariability
                ? new UnitVariabilityCalculator(
                    ModuleConfig.UnitVariabilityModel,
                    ModuleConfig.UnitVariabilityType,
                    ModuleConfig.EstimatesNature,
                    ModuleConfig.DefaultFactorLow,
                    ModuleConfig.DefaultFactorMid,
                    ModuleConfig.MeanValueCorrectionType,
                    ModuleConfig.CorrelationType,
                    data.UnitVariabilityDictionary
                )
                : null;

            // Create an individual day intakes pruner to prune individual day intakes
            IIndividualDayIntakePruner individualDayIntakePruner;
            if (ModuleConfig.DietaryExposuresDetailsLevel == DietaryExposuresDetailsLevel.OnlyRiskDrivers
                && data.ScreeningResult?.ScreeningResultsPerFoodCompound != null) {
                individualDayIntakePruner = new ScreeningToAggregateIntakesPruner(
                    data.ScreeningResult?.ScreeningResultsPerFoodCompound,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities
                );
            } else if (ModuleConfig.DietaryExposuresDetailsLevel == DietaryExposuresDetailsLevel.OmitFoodsAsEaten) {
                individualDayIntakePruner = new AggregateByFoodAsMeasuredPruner();
            } else {
                individualDayIntakePruner = new VoidPruner();
            }

            // Processing factor provider
            var processingFactorProvider = ModuleConfig.IsProcessing
                ? new ProcessingFactorProvider(
                    data.ProcessingFactorModels,
                    ModuleConfig.UseDefaultMissingProcessingFactor,
                    ModuleConfig.DefaultMissingProcessingFactor
                ) : null;

            // Generate individual-day intakes
            localProgress.Update("Computing dietary exposures", 35);
            var intakeCalculatorFactory = new IntakeCalculatorFactory(
                ModuleConfig.IsSampleBased,
                ModuleConfig.MaximiseCoOccurrenceHighResidues,
                ModuleConfig.IsSingleSamplePerDay,
                ModuleConfig.NumberOfMonteCarloIterations,
                ModuleConfig.ExposureType,
                ModuleConfig.TotalDietStudy,
                ModuleConfig.ReductionToLimitScenario
            );
            var intakeCalculator = intakeCalculatorFactory
                .Create(
                    processingFactorProvider,
                    result.TdsReductionFactors,
                    residueGenerator,
                    unitVariabilityCalculator,
                    individualDayIntakePruner,
                    data.ConsumptionsByModelledFood,
                    substances,
                    data.ConcentrationModels
                );

            data.DietaryIndividualDayIntakes = intakeCalculator
                .CalculateDietaryIntakes(
                    simulatedIndividualDays,
                    new ProgressState(progressReport.CancellationToken),
                    ModuleConfig.RandomSeed // TODO: refactor (main seed currently splits up in different random sources in the calculator itself)
                );
            result.DietaryIndividualDayIntakes = data.DietaryIndividualDayIntakes;

            // Compute exposures by substance
            var exposurePerCompoundRecords = intakeCalculator.ComputeExposurePerCompoundRecords(data.DietaryIndividualDayIntakes);
            if (ModuleConfig.ImputeExposureDistributions) {
                var exposureImputationRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DE_DrawImputedExposures));
                var exposureImputationCalculator = new DietaryExposureImputationCalculator();
                data.DietaryIndividualDayIntakes = exposureImputationCalculator
                    .Impute(
                        ModuleConfig.ExposureType,
                        exposurePerCompoundRecords,
                        data.CompoundResidueCollections,
                        substances,
                        data.DietaryIndividualDayIntakes,
                        exposureImputationRandomGenerator
                    );
                result.DietaryIndividualDayIntakes = data.DietaryIndividualDayIntakes;
            }
            result.ExposurePerCompoundRecords = exposurePerCompoundRecords;

            localProgress.Update(80);
            if (ModuleConfig.ExposureType == ExposureType.Chronic
                && (substances.Count == 1 || data.CorrectedRelativePotencyFactors != null)
            ) {
                var factory = new IntakeModelFactory(
                    ModuleConfig.GetFrequencyModelCalculationSettings(),
                    ModuleConfig.GetAmountModelCalculationSettings(),
                    ModuleConfig.GetISUFModelCalculationSettings(),
                    ModuleConfig.NumberOfMonteCarloIterations,
                    ModuleConfig.IntakeModelPredictionIntervals,
                    ModuleConfig.IntakeExtraPredictionLevels.ToArray(),
                    ModuleConfig.FrequencyModelDispersion,
                    ModuleConfig.AmountModelVarianceRatio);

                var simpleIndividualDayIntakesCalculator = new SimpleIndividualDayIntakesCalculator(
                    substances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    ModuleConfig.IsPerPerson,
                    null
                );
                var simpleIndividualDayIntakes = simpleIndividualDayIntakesCalculator
                    .Compute(data.DietaryIndividualDayIntakes);
                var observedIndividualMeans = OIMCalculator
                    .CalculateObservedIndividualMeans(simpleIndividualDayIntakes);
                if (ModuleConfig.IntakeFirstModelThenAdd) {
                    var compositeIntakeModel = factory.CreateCompositeIntakeModel(
                        data.DietaryIndividualDayIntakes,
                        data.ModelledFoods,
                        ModuleConfig.IntakeModelsPerCategory
                    );
                    foreach (var subModel in compositeIntakeModel.PartialModels) {
                        var categoryIndividualDayIntakesCalculator = new SimpleIndividualDayIntakesCalculator(
                            substances,
                            data.CorrectedRelativePotencyFactors,
                            data.MembershipProbabilities,
                            ModuleConfig.IsPerPerson,
                            subModel.FoodsAsMeasured
                        );
                        if (!(subModel.IntakeModel is OIMModel)) {
                            var categoryIndividualDayIntakes = categoryIndividualDayIntakesCalculator
                                .Compute(data.DietaryIndividualDayIntakes);
                            subModel.IntakeModel.CalculateParameters(categoryIndividualDayIntakes);
                        }
                        subModel.IndividualIntakes = categoryIndividualDayIntakesCalculator
                            .ComputeObservedIndividualMeans(data.DietaryIndividualDayIntakes);
                    }

                    var modelThenAddCalculator = new ModelThenAddUsualIntakesCalculator(progressReport?.CancellationToken);
                    var mtaIntakeResults = modelThenAddCalculator.CalculateUsualIntakes(
                        compositeIntakeModel,
                        data.DietaryIndividualDayIntakes,
                        ModuleConfig.NumberOfMonteCarloIterations,
                        ModuleConfig.FrequencyModelCovariateModelType,
                        ModuleConfig.AmountModelCovariateModelType,
                        RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DE_DrawModelBasedExposures),
                        RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DE_DrawModelAssistedExposures)
                    );
                    result.IntakeModel = compositeIntakeModel;
                    result.DietaryObservedIndividualMeans = observedIndividualMeans;
                    result.DietaryModelBasedIntakeResults = mtaIntakeResults.DietaryModelBasedIntakeResults;
                    result.DietaryModelAssistedIntakes = mtaIntakeResults.DietaryModelAssistedIntakes;
                } else {
                    var intakeModel = factory.CreateIntakeModel(
                        data.DietaryIndividualDayIntakes,
                        ModuleConfig.IntakeCovariateModelling,
                        ModuleConfig.ExposureType,
                        ModuleConfig.IntakeModelType,
                        ModuleConfig.AmountModelTransformType
                    );
                    intakeModel.CalculateParameters(simpleIndividualDayIntakes);
                    result.DesiredIntakeModelType = ModuleConfig.IntakeModelType;
                    if (intakeModel is LNNModel) {
                        result.DesiredIntakeModelType = (intakeModel as LNNModel).FallBackModel;
                    }
                    var usualIntakeCalculator = new UsualIntakesCalculator();
                    var intakeResults = usualIntakeCalculator
                        .CalculateUsualIntakes(
                            intakeModel,
                            ModuleConfig.ExposureType,
                            ModuleConfig.IntakeCovariateModelling,
                            RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DE_DrawModelBasedExposures),
                            RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DE_DrawModelAssistedExposures)
                        );
                    result.DietaryObservedIndividualMeans = observedIndividualMeans;
                    result.IntakeModel = intakeModel;
                    result.DietaryModelAssistedIntakes = intakeResults?.ModelAssistedIntakes;
                    result.DietaryModelBasedIntakeResults = intakeResults?.ModelBasedIntakeResults;
                    result.DietaryConditionalUsualIntakeResults = intakeResults?.ConditionalUsualIntakes;
                    result.IndividualModelAssistedIntakes = intakeResults?.IndividualModelAssistedIntakes;
                }
            } else {
                if (ModuleConfig.ExposureType == ExposureType.Chronic && ModuleConfig.IntakeModelType != IntakeModelType.OIM) {
                    throw new Exception("Parametric exposure models are not allowed for a multiple substance analysis without a reference substance");
                }

            }

            localProgress.Update(100);

            if (substances.Count > 1 && data.CorrectedRelativePotencyFactors != null
                && ModuleConfig.TargetDoseLevelType == TargetLevelType.External
                && ModuleConfig.McrAnalysis
            ) {
                var exposureMatrixBuilder = new ExposureMatrixBuilder(
                    substances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    ModuleConfig.ExposureType,
                    ModuleConfig.IsPerPerson,
                    ModuleConfig.McrExposureApproachType,
                    ModuleConfig.McrCalculationTotalExposureCutOff,
                    ModuleConfig.McrCalculationRatioCutOff
                );
                result.ExposureMatrix = exposureMatrixBuilder.Compute(
                    result.DietaryIndividualDayIntakes,
                    result.DietaryExposureUnit
                );
                result.DriverSubstances = DriverSubstanceCalculator.CalculateExposureDrivers(result.ExposureMatrix);
            }
            return result;
        }

        protected override void summarizeActionResult(DietaryExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var summarizer = new DietaryExposuresSummarizer(ModuleConfig, progressReport);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
        }

        protected override void updateSimulationData(ActionData data, DietaryExposuresActionResult result) {
            data.DesiredIntakeModelType = result.DesiredIntakeModelType;
            data.DietaryIndividualDayIntakes = result.DietaryIndividualDayIntakes;
            data.DietaryObservedIndividualMeans = result.DietaryObservedIndividualMeans;
            data.DietaryModelAssistedIntakes = result.DietaryModelAssistedIntakes;
            data.DrillDownDietaryIndividualIntakes = result.IndividualModelAssistedIntakes;
            data.DietaryExposuresIntakeModel = result.IntakeModel;
            data.DietaryModelBasedIntakeResults = result.DietaryModelBasedIntakeResults;
            data.DietaryExposureUnit = result.DietaryExposureUnit;
            data.TdsReductionFactors = result.TdsReductionFactors ?? data.TdsReductionFactors;
            data.TdsReductionScenarioAnalysisFoods = result.TdsReductionScenarioAnalysisFoods ?? data.TdsReductionScenarioAnalysisFoods;
        }

        protected override DietaryExposuresActionResult runUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            var result = new DietaryExposuresActionResult();
            var uncertaintyFactorialResponses = new List<double>();
            var substances = data.ActiveSubstances;

            result.DietaryExposureUnit = data.DietaryExposureUnit;

            // Create residue generator
            // Create residue generator
            var residueGeneratorFactory = new ResidueGeneratorFactory(
                ModuleConfig.UseOccurrencePatternsForResidueGeneration,
                ModuleConfig.SetMissingAgriculturalUseAsUnauthorized,
                ModuleConfig.IsSampleBased,
                ModuleConfig.DefaultConcentrationModel != ConcentrationModelType.Empirical,
                ModuleConfig.ExposureType,
                ModuleConfig.NonDetectsHandlingMethod
            );
            var residueGenerator = residueGeneratorFactory.Create(
                data.MonteCarloSubstanceSampleCollections?.ToDictionary(r => r.Food),
                data.ConcentrationModels,
                data.CumulativeConcentrationModels,
                data.CorrectedRelativePotencyFactors,
                data.MarginalOccurrencePatterns
            );

            // Unit variability model calculator
            var unitVariabilityCalculator = ModuleConfig.ExposureType == ExposureType.Acute && ModuleConfig.UseUnitVariability
                ? new UnitVariabilityCalculator(
                    ModuleConfig.UnitVariabilityModel,
                    ModuleConfig.UnitVariabilityType,
                    ModuleConfig.EstimatesNature,
                    ModuleConfig.DefaultFactorLow,
                    ModuleConfig.DefaultFactorMid,
                    ModuleConfig.MeanValueCorrectionType,
                    ModuleConfig.CorrelationType,
                    data.UnitVariabilityDictionary
                )
                : null;

            // Create an individual day intakes pruner to prune individual day intakes
            IIndividualDayIntakePruner individualDayIntakePruner;
            if (ModuleConfig.DietaryExposuresDetailsLevel == DietaryExposuresDetailsLevel.OnlyRiskDrivers
                && data.ScreeningResult?.ScreeningResultsPerFoodCompound != null) {
                individualDayIntakePruner = new ScreeningToAggregateIntakesPruner(
                    data.ScreeningResult?.ScreeningResultsPerFoodCompound,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities
                );
            } else if (ModuleConfig.DietaryExposuresDetailsLevel == DietaryExposuresDetailsLevel.OmitFoodsAsEaten) {
                individualDayIntakePruner = new AggregateByFoodAsMeasuredPruner();
            } else {
                individualDayIntakePruner = new VoidPruner();
            }

            // Processing factor provider
            var processingFactorProvider = ModuleConfig.IsProcessing
                ? new ProcessingFactorProvider(
                    data.ProcessingFactorModels,
                    ModuleConfig.UseDefaultMissingProcessingFactor,
                    ModuleConfig.DefaultMissingProcessingFactor
                ) : null;

            // Create intake calculator
            var intakeCalculatorFactory = new IntakeCalculatorFactory(
                ModuleConfig.IsSampleBased,
                ModuleConfig.MaximiseCoOccurrenceHighResidues,
                ModuleConfig.IsSingleSamplePerDay,
                ModuleConfig.UncertaintyIterationsPerResampledSet,
                ModuleConfig.ExposureType,
                ModuleConfig.TotalDietStudy,
                ModuleConfig.ReductionToLimitScenario
            );
            var intakeCalculator = intakeCalculatorFactory
                .Create(
                    processingFactorProvider,
                    data.TdsReductionFactors,
                    residueGenerator,
                    unitVariabilityCalculator,
                    individualDayIntakePruner,
                    data.ConsumptionsByModelledFood,
                    substances,
                    data.ConcentrationModels
                );

            if (factorialSet.Contains(UncertaintySource.Portions)
                && intakeCalculator.UnitWeightGenerator != null
            ) {
                localProgress.Update("Resampling portions");
                intakeCalculator.ModelConsumptionAmountUncertainty = true;
                intakeCalculator.UnitWeightGenerator
                    .ModelUncertainty(uncertaintySourceGenerators[UncertaintySource.Portions]);
            }

            // Create simulated individuals
            var individualsRandomGenerator = new McraRandomGenerator(
                RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DE_DrawIndividuals)
            );

            var populationGeneratorFactory = new PopulationGeneratorFactory(
                ModuleConfig.ExposureType,
                ModuleConfig.IsSurveySampling,
                ModuleConfig.UncertaintyIterationsPerResampledSet
            );
            var populationGenerator = populationGeneratorFactory.Create();
            var simulatedIndividualDays = populationGenerator
                .CreateSimulatedIndividualDays(
                    data.ModelledFoodConsumers,
                    data.ModelledFoodConsumerDays,
                    individualsRandomGenerator
                );
            IndividualDaysGenerator.ImputeBodyWeight(simulatedIndividualDays.Select(d => d.SimulatedIndividual).Distinct());
            result.SimulatedIndividualDays = simulatedIndividualDays;

            // Compute exposures of uncertainty run
            var uncertaintyDietaryIntakes = intakeCalculator
                .CalculateDietaryIntakes(
                    simulatedIndividualDays,
                    new ProgressState(progressReport.CancellationToken),
                    ModuleConfig.RandomSeed // TODO: refactor (main seed currently splits up in different random sources in the calculator itself)
                );
            result.DietaryIndividualDayIntakes = uncertaintyDietaryIntakes;
            data.DietaryIndividualDayIntakes = uncertaintyDietaryIntakes;

            // Exposure imputation
            if (ModuleConfig.ImputeExposureDistributions) {
                var exposurePerCompoundRecords = intakeCalculator
                    .ComputeExposurePerCompoundRecords(data.DietaryIndividualDayIntakes);
                var exposureImputationCalculator = new DietaryExposureImputationCalculator();
                var exposureImputationRandomGenerator = new McraRandomGenerator(
                    RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DE_DrawImputedExposures)
                );
                if (factorialSet.Contains(UncertaintySource.ImputeExposureDistributions)) {
                    uncertaintyDietaryIntakes = exposureImputationCalculator
                        .ImputeUncertaintyRun(
                            ModuleConfig.ExposureType,
                            exposurePerCompoundRecords,
                            data.CompoundResidueCollections,
                            substances,
                            data.DietaryIndividualDayIntakes,
                            exposureImputationRandomGenerator,
                            uncertaintySourceGenerators[UncertaintySource.ImputeExposureDistributions]
                        );
                } else {
                    uncertaintyDietaryIntakes = exposureImputationCalculator
                        .Impute(
                            ModuleConfig.ExposureType,
                            exposurePerCompoundRecords,
                            data.CompoundResidueCollections,
                            substances,
                            data.DietaryIndividualDayIntakes,
                            exposureImputationRandomGenerator
                        );
                }
            }

            if (substances.Count == 1 || data.CorrectedRelativePotencyFactors != null) {
                if (ModuleConfig.ExposureType == ExposureType.Chronic) {
                    var factory = new IntakeModelFactory(
                        ModuleConfig.GetFrequencyModelCalculationSettings(),
                        ModuleConfig.GetAmountModelCalculationSettings(),
                        ModuleConfig.GetISUFModelCalculationSettings(),
                        ModuleConfig.UncertaintyIterationsPerResampledSet,
                        ModuleConfig.IntakeModelPredictionIntervals,
                        ModuleConfig.IntakeExtraPredictionLevels.ToArray(),
                        ModuleConfig.FrequencyModelDispersion,
                        ModuleConfig.AmountModelVarianceRatio);
                    var simpleIndividualDayIntakesCalculator = new SimpleIndividualDayIntakesCalculator(
                        substances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        ModuleConfig.IsPerPerson,
                        null
                    );
                    var simpleIndividualDayIntakes = simpleIndividualDayIntakesCalculator
                        .Compute(uncertaintyDietaryIntakes);
                    var observedIndividualMeans = OIMCalculator
                        .CalculateObservedIndividualMeans(simpleIndividualDayIntakes);
                    if (ModuleConfig.IntakeFirstModelThenAdd) {
                        var compositeIntakeModel = factory
                            .CreateCompositeIntakeModel(
                                uncertaintyDietaryIntakes,
                                data.ModelledFoods,
                                ModuleConfig.IntakeModelsPerCategory
                            );
                        foreach (var subModel in compositeIntakeModel.PartialModels) {
                            var categoryIndividualDayIntakesCalculator = new SimpleIndividualDayIntakesCalculator(
                                substances,
                                data.CorrectedRelativePotencyFactors,
                                data.MembershipProbabilities,
                                ModuleConfig.IsPerPerson,
                                subModel.FoodsAsMeasured
                            );
                            if (!(subModel.IntakeModel is OIMModel)) {
                                var categoryIndividualDayIntakes = categoryIndividualDayIntakesCalculator
                                    .Compute(uncertaintyDietaryIntakes);
                                subModel.IntakeModel.CalculateParameters(categoryIndividualDayIntakes);
                            }
                            subModel.IndividualIntakes = categoryIndividualDayIntakesCalculator
                                .ComputeObservedIndividualMeans(uncertaintyDietaryIntakes);
                        }

                        // Model-then-add random generator
                        var modelThenAddCalculator = new ModelThenAddUsualIntakesCalculator(progressReport?.CancellationToken);
                        var mtaIntakeResults = modelThenAddCalculator.CalculateUsualIntakes(
                            compositeIntakeModel,
                            uncertaintyDietaryIntakes,
                            ModuleConfig.UncertaintyIterationsPerResampledSet,
                            ModuleConfig.FrequencyModelCovariateModelType,
                            ModuleConfig.AmountModelCovariateModelType,
                            RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DE_DrawModelBasedExposures),
                            RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DE_DrawModelAssistedExposures)
                        );

                        result.IntakeModel = compositeIntakeModel;
                        result.DietaryObservedIndividualMeans = observedIndividualMeans;
                        result.DietaryModelBasedIntakeResults = mtaIntakeResults.DietaryModelBasedIntakeResults;
                        result.DietaryModelAssistedIntakes = mtaIntakeResults.DietaryModelAssistedIntakes;
                        uncertaintyFactorialResponses = mtaIntakeResults.DietaryModelBasedIntakes
                            ?? mtaIntakeResults.DietaryModelAssistedIntakes
                                .Select(c => c.DietaryIntakePerMassUnit).ToList();
                    } else {
                        var intakeModel = factory.CreateIntakeModel(
                            uncertaintyDietaryIntakes,
                            ModuleConfig.IntakeCovariateModelling,
                            ModuleConfig.ExposureType,
                            data.DesiredIntakeModelType,
                            ModuleConfig.AmountModelTransformType
                        );
                        result.DesiredIntakeModelType = data.DesiredIntakeModelType;
                        intakeModel.CalculateParameters(simpleIndividualDayIntakes);
                        var usualIntakeCalculator = new UsualIntakesCalculator();
                        var intakeResults = usualIntakeCalculator.CalculateUsualIntakes(
                            intakeModel,
                            ModuleConfig.ExposureType,
                            ModuleConfig.IntakeCovariateModelling,
                            RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DE_DrawModelBasedExposures),
                            RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.DE_DrawModelAssistedExposures)
                        );

                        result.DietaryObservedIndividualMeans = observedIndividualMeans;
                        result.IntakeModel = intakeModel;
                        result.DietaryModelAssistedIntakes = intakeResults?.ModelAssistedIntakes;
                        result.DietaryModelBasedIntakeResults = intakeResults?.ModelBasedIntakeResults;
                        result.DietaryConditionalUsualIntakeResults = intakeResults?.ConditionalUsualIntakes;
                        result.IndividualModelAssistedIntakes = intakeResults?.IndividualModelAssistedIntakes;

                        if (intakeModel is ISUFModel) {
                            uncertaintyFactorialResponses = (intakeModel as ISUFModel).UsualIntakeResult.UsualIntakes.Select(c => c.UsualIntake).ToList();
                        } else {
                            var modelBasedIntakes = intakeResults?.ModelBasedIntakeResults?.SelectMany(c => c.ModelBasedIntakes).ToList();
                            uncertaintyFactorialResponses = modelBasedIntakes ?? observedIndividualMeans?.Select(r => r.DietaryIntakePerMassUnit).ToList();
                        }
                    }
                } else {
                    if (data.ActiveSubstances.Count > 1) {
                        uncertaintyFactorialResponses = result.DietaryIndividualDayIntakes
                            .Select(c => c.TotalExposurePerMassUnit(data.CorrectedRelativePotencyFactors, data.MembershipProbabilities, ModuleConfig.IsPerPerson))
                            .ToList();
                    } else {
                        uncertaintyFactorialResponses = result.DietaryIndividualDayIntakes
                            .Select(c => c.GetSubstanceTotalExposurePerMassUnit(data.ActiveSubstances.First(), ModuleConfig.IsPerPerson))
                            .ToList();
                    }
                }
            }

            // Update factorial results
            result.FactorialResult = new DietaryExposuresFactorialResult() {
                Percentages = ModuleConfig.SelectedPercentiles.ToArray(),
                Percentiles = uncertaintyFactorialResponses?
                    .PercentilesWithSamplingWeights(null, ModuleConfig.SelectedPercentiles).ToList()
            };

            localProgress.Update(100);
            return result;
        }

        protected override void writeOutputData(IRawDataWriter rawDataWriter, ActionData data, DietaryExposuresActionResult result) {
            var outputWriter = new DietaryExposuresOutputWriter();
            outputWriter.WriteOutputData(ModuleConfig, data, result, rawDataWriter);
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, DietaryExposuresActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new DietaryExposuresSummarizer(ModuleConfig);
            summarizer.SummarizeUncertain(_actionSettings, actionResult, data, header);
            localProgress.Update(100);
        }

        protected override void updateSimulationDataUncertain(ActionData data, DietaryExposuresActionResult result) {
            updateSimulationData(data, result);
        }

        protected override void writeOutputDataUncertain(IRawDataWriter rawDataWriter, ActionData data, DietaryExposuresActionResult result, int idBootstrap) {
            var outputWriter = new DietaryExposuresOutputWriter();
            outputWriter.UpdateOutputData(ModuleConfig, rawDataWriter, data, result, idBootstrap);
        }

        public override void SummarizeUncertaintyFactorial(
            UncertaintyFactorialDesign uncertaintyFactorial,
            List<UncertaintyFactorialResultRecord> factorialResult,
            SectionHeader header
        ) {
            if (!factorialResult.Any(r => r.ResultRecord is DietaryExposuresFactorialResult)) {
                return;
            }

            // Get the factorial percentile results and the percentages
            var factorialPercentilesResults = factorialResult
                .Where(r => r.ResultRecord is DietaryExposuresFactorialResult)
                .Select(r => (r.Tags, (r.ResultRecord as DietaryExposuresFactorialResult).Percentiles))
                .ToList();
            var percentages = (factorialResult
                .First(r => r.ResultRecord is DietaryExposuresFactorialResult).ResultRecord as DietaryExposuresFactorialResult)
                .Percentages;

            // Compute percentiles factorial
            var percentilesFactorialCalculator = new PercentilesUncertaintyFactorialCalculator();
            var percentilesFactorialResults = percentilesFactorialCalculator.Compute(
                factorialPercentilesResults,
                percentages,
                uncertaintyFactorial.UncertaintySources,
                uncertaintyFactorial.DesignMatrix
            );

            // Summarize percentiles factorial results
            var subHeader = header.GetSubSectionHeader<DietaryExposuresSummarySection>();
            var uncertaintyFactorialSection = new UncertaintyFactorialSection();
            var subSubHeader = subHeader.AddSubSectionHeaderFor(uncertaintyFactorialSection, "Uncertainty factorial", 150);
            uncertaintyFactorialSection.Summarize(percentilesFactorialResults, percentages);
            subSubHeader.SaveSummarySection(uncertaintyFactorialSection);
        }

        protected override IActionComparisonData loadActionComparisonData(ICompiledDataManager compiledDataManager) {
            var result = new DietaryExposuresActionComparisonData() {
                DietaryExposureModels = compiledDataManager.GetAllDietaryExposureModels()?.Values
            };
            return result;
        }

        public override void SummarizeComparison(ICollection<IActionComparisonData> comparisonData, SectionHeader header) {
            var models = comparisonData
                .Where(r => (r as DietaryExposuresActionComparisonData).DietaryExposureModels?.Count > 0)
                .Select(r => {
                    var result = (r as DietaryExposuresActionComparisonData).DietaryExposureModels.First();
                    result.Code = r.IdResultSet;
                    result.Name = r.NameResultSet;
                    return result;
                })
                .ToList();
            var summarizer = new DietaryExposuresCombinedActionSummarizer();
            summarizer.Summarize(models, header);
        }
    }
}
