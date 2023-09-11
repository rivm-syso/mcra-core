using MCRA.Data.Management;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.ActionComparison;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.DietaryExposureImputationCalculation;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDayPruning;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Calculators.IntakeModelling.IndividualAmountCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.ModelThenAddIntakeModelCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.OIMCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.UsualIntakeCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation;
using MCRA.Simulation.Calculators.PopulationGeneration;
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

        public DietaryExposuresActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var isMultipleSubstances = _project.AssessmentSettings.MultipleSubstances
                && !(_project.LoopScopingTypes?.Contains(ScopingType.Compounds) ?? false);
            var isCumulative = isMultipleSubstances && _project.AssessmentSettings.Cumulative;
            var isRiskBasedMcr = _project.AssessmentSettings.MultipleSubstances
                && _project.MixtureSelectionSettings.IsMcrAnalysis
                && _project.MixtureSelectionSettings.McrExposureApproachType == ExposureApproachType.RiskBased;
            var isTotalDietStudy = _project.AssessmentSettings.TotalDietStudy && _project.AssessmentSettings.ExposureType == ExposureType.Chronic;
            _actionInputRequirements[ActionType.Effects].IsRequired = isCumulative;
            _actionInputRequirements[ActionType.Effects].IsVisible = isCumulative;
            var useUnitVariabilityFactors = _project.AssessmentSettings.ExposureType == ExposureType.Acute && _project.UnitVariabilitySettings.UseUnitVariability;
            _actionInputRequirements[ActionType.UnitVariabilityFactors].IsRequired = useUnitVariabilityFactors;
            _actionInputRequirements[ActionType.UnitVariabilityFactors].IsVisible = useUnitVariabilityFactors;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsRequired = isCumulative || isRiskBasedMcr;
            _actionInputRequirements[ActionType.RelativePotencyFactors].IsVisible = isCumulative || isRiskBasedMcr;
            var useProcessingFactors = !isTotalDietStudy && _project.ConcentrationModelSettings.IsProcessing;
            _actionInputRequirements[ActionType.ProcessingFactors].IsRequired = useProcessingFactors;
            _actionInputRequirements[ActionType.ProcessingFactors].IsVisible = useProcessingFactors;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = isMultipleSubstances;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = isMultipleSubstances;
            var isCumulativeScreening = isCumulative && _project.DietaryIntakeCalculationSettings.DietaryExposuresDetailsLevel == DietaryExposuresDetailsLevel.OnlyRiskDrivers;
            _actionInputRequirements[ActionType.HighExposureFoodSubstanceCombinations].IsRequired = isCumulativeScreening;
            _actionInputRequirements[ActionType.HighExposureFoodSubstanceCombinations].IsVisible = isCumulativeScreening;
            var useOccurrencePatterns = !_project.ConcentrationModelSettings.IsSampleBased
                && _project.AgriculturalUseSettings.UseOccurrencePatternsForResidueGeneration;
            _actionInputRequirements[ActionType.OccurrencePatterns].IsRequired = useOccurrencePatterns;
            _actionInputRequirements[ActionType.OccurrencePatterns].IsVisible = useOccurrencePatterns;

            var isTdsReductionToLimitScenario = _project.AssessmentSettings.TotalDietStudy && _project.ScenarioAnalysisSettings.UseScenario;
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
            if (_project.UncertaintyAnalysisSettings.ReSamplePortions) {
                result.Add(UncertaintySource.Portions);
            }
            if (_project.UncertaintyAnalysisSettings.ReSampleImputationExposureDistributions) {
                result.Add(UncertaintySource.ImputeExposureDistributions);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new DietaryExposuresSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override DietaryExposuresActionResult run(ActionData data, CompositeProgressState progressReport) {
            var settings = new DietaryExposuresModuleSettings(_project, false);
            var substances = data.ActiveSubstances;

            var localProgress = progressReport.NewProgressState(100);
            var result = new DietaryExposuresActionResult();
            result.DietaryExposureUnit = TargetUnit.CreateDietaryExposureUnit(
                data.ConsumptionUnit, 
                data.ConcentrationUnit, 
                data.BodyWeightUnit, 
                settings.IsPerPerson
            );

            // Create individual days
            localProgress.Update("Generating individual days", 30);
            var individualsRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.DE_DrawIndividuals));
            var populationGeneratorFactory = new PopulationGeneratorFactory(settings);
            var populationGenerator = populationGeneratorFactory.Create();
            var simulatedIndividualDays = populationGenerator.CreateSimulatedIndividualDays(
                data.ModelledFoodConsumers,
                data.ModelledFoodConsumerDays,
                individualsRandomGenerator
            );
            result.SimulatedIndividualDays = simulatedIndividualDays;

            // Select only TDS compositions that are found in conversion algorithm
            if (settings.ExposureType == ExposureType.Chronic && settings.TotalDietStudy && settings.UseScenario) {
                localProgress.Update("Computing TDS reduction factors", 33);
                var tdsReductionFactorsCalculator = new TdsReductionFactorsCalculator(data.ConcentrationDistributions);
                result.TdsReductionScenarioAnalysisFoods = settings.SelectedScenarioAnalysisFoods
                    .Where(r => data.AllFoodsByCode.ContainsKey(r))
                    .Select(r => data.AllFoodsByCode[r])
                    .ToList();
                result.TdsReductionFactors = tdsReductionFactorsCalculator
                    .CalculateReductionFactors(result.TdsReductionScenarioAnalysisFoods);
            }

            // Create residue generator
            var residueGeneratorFactory = new ResidueGeneratorFactory(settings);
            var residueGenerator = residueGeneratorFactory.Create(
                data.MonteCarloSubstanceSampleCollections?.ToDictionary(r => r.Food),
                data.ConcentrationModels,
                data.CumulativeConcentrationModels,
                data.CorrectedRelativePotencyFactors,
                data.MarginalOccurrencePatterns
            );

            //  Unit variability model calculator
            var unitVariabilityCalculator = settings.ExposureType == ExposureType.Acute && settings.UseUnitVariability
                ? new UnitVariabilityCalculator(settings, data.UnitVariabilityDictionary)
                : null;

            // Create an individual day intakes pruner to prune individual day intakes
            IIndividualDayIntakePruner individualDayIntakePruner;
            if (settings.DietaryExposuresDetailsLevel == DietaryExposuresDetailsLevel.OnlyRiskDrivers
                && data.ScreeningResult?.ScreeningResultsPerFoodCompound != null) {
                individualDayIntakePruner = new ScreeningToAggregateIntakesPruner(
                    data.ScreeningResult?.ScreeningResultsPerFoodCompound,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities
                );
            } else if (settings.DietaryExposuresDetailsLevel == DietaryExposuresDetailsLevel.OmitFoodsAsEaten) {
                individualDayIntakePruner = new AggregateByFoodAsMeasuredPruner();
            } else {
                individualDayIntakePruner = new VoidPruner();
            }

            // Generate individual-day intakes
            localProgress.Update("Computing dietary exposures", 35);
            var intakeCalculatorFactory = new IntakeCalculatorFactory(settings);
            var intakeCalculator = intakeCalculatorFactory.Create(
                data.ProcessingFactorModels,
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
                    _project.MonteCarloSettings.RandomSeed // TODO: refactor (main seed currently splits up in different random sources in the calculator itself)
                );
            result.DietaryIndividualDayIntakes = data.DietaryIndividualDayIntakes;

            // Compute exposures by substance
            var exposurePerCompoundRecords = intakeCalculator.ComputeExposurePerCompoundRecords(data.DietaryIndividualDayIntakes);
            if (settings.ImputeExposureDistributions) {
                var exposureImputationRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.DE_DrawImputedExposures));
                var exposureImputationCalculator = new DietaryExposureImputationCalculator();
                data.DietaryIndividualDayIntakes = exposureImputationCalculator
                    .Impute(
                        settings.ExposureType,
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
            if (settings.ExposureType == ExposureType.Chronic
                && (substances.Count == 1 || data.CorrectedRelativePotencyFactors != null)
            ) {
                var factory = new IntakeModelFactory(
                    settings.FrequencyModelCalculationSettings,
                    settings.AmountModelCalculationSettings,
                    settings.ISUFModelCalculationSettings,
                    settings.NumberOfMonteCarloIterations,
                    settings.Intervals,
                    settings.ExtraPredictionLevels,
                    settings.Dispersion,
                    settings.VarianceRatio);

                var simpleIndividualDayIntakesCalculator = new SimpleIndividualDayIntakesCalculator(
                    substances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    settings.IsPerPerson,
                    null
                );
                var simpleIndividualDayIntakes = simpleIndividualDayIntakesCalculator
                    .Compute(data.DietaryIndividualDayIntakes);
                var observedIndividualMeans = OIMCalculator
                    .CalculateObservedIndividualMeans(simpleIndividualDayIntakes);
                if (settings.FirstModelThenAdd) {
                    var compositeIntakeModel = factory.CreateCompositeIntakeModel(
                        data.DietaryIndividualDayIntakes,
                        data.ModelledFoods,
                        settings.IntakeModelsPerCategory
                    );
                    foreach (var subModel in compositeIntakeModel.PartialModels) {
                        var categoryIndividualDayIntakesCalculator = new SimpleIndividualDayIntakesCalculator(
                            substances,
                            data.CorrectedRelativePotencyFactors,
                            data.MembershipProbabilities,
                            settings.IsPerPerson,
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
                        settings.NumberOfMonteCarloIterations,
                        settings.FrequencyModelCalculationSettings.CovariateModelType,
                        settings.AmountModelCalculationSettings.CovariateModelType,
                        RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.DE_DrawModelBasedExposures),
                        RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.DE_DrawModelAssistedExposures)
                    );
                    result.IntakeModel = compositeIntakeModel;
                    result.DietaryObservedIndividualMeans = observedIndividualMeans;
                    result.DietaryModelBasedIntakeResults = mtaIntakeResults.DietaryModelBasedIntakeResults;
                    result.DietaryModelAssistedIntakes = mtaIntakeResults.DietaryModelAssistedIntakes;
                } else {
                    var intakeModel = factory.CreateIntakeModel(
                        data.DietaryIndividualDayIntakes,
                        settings.CovariateModelling,
                        settings.ExposureType,
                        settings.IntakeModelType,
                        settings.TransformType
                    );
                    intakeModel.CalculateParameters(simpleIndividualDayIntakes);
                    result.DesiredIntakeModelType = settings.IntakeModelType;
                    if (intakeModel is LNNModel) {
                        result.DesiredIntakeModelType = (intakeModel as LNNModel).FallBackModel;
                    }
                    var usualIntakeCalculator = new UsualIntakesCalculator();
                    var intakeResults = usualIntakeCalculator
                        .CalculateUsualIntakes(
                            intakeModel,
                            settings.ExposureType,
                            settings.CovariateModelling,
                            RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.DE_DrawModelBasedExposures),
                            RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.DE_DrawModelAssistedExposures)
                        );
                    result.DietaryObservedIndividualMeans = observedIndividualMeans;
                    result.IntakeModel = intakeModel;
                    result.DietaryModelAssistedIntakes = intakeResults?.ModelAssistedIntakes;
                    result.DietaryModelBasedIntakeResults = intakeResults?.ModelBasedIntakeResults;
                    result.DietaryConditionalUsualIntakeResults = intakeResults?.ConditionalUsualIntakes;
                    result.IndividualModelAssistedIntakes = intakeResults?.IndividualModelAssistedIntakes;
                }
            } else {
                if (settings.ExposureType == ExposureType.Chronic && settings.IntakeModelType != IntakeModelType.OIM) {
                    throw new Exception("Parametric exposure models are not allowed for a multiple substance analysis without a reference substance");
                }

            }

            localProgress.Update(100);

            if (substances.Count > 1 && data.CorrectedRelativePotencyFactors != null
                && _project.EffectSettings.TargetDoseLevelType == TargetLevelType.External
                && _project.MixtureSelectionSettings.IsMcrAnalysis
            ) {
                var exposureMatrixBuilder = new ExposureMatrixBuilder(
                    substances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    settings.ExposureType,
                    settings.IsPerPerson,
                    settings.ExposureApproachType,
                    settings.TotalExposureCutOff,
                    settings.RatioCutOff
                 );
                result.ExposureMatrix = exposureMatrixBuilder.Compute(result.DietaryIndividualDayIntakes, data.DietaryExposureUnit);
                result.DriverSubstances = DriverSubstanceCalculator.CalculateExposureDrivers(result.ExposureMatrix);
            }
            return result;
        }

        protected override void summarizeActionResult(DietaryExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var summarizer = new DietaryExposuresSummarizer(progressReport);
            summarizer.Summarize(_project, actionResult, data, header, order);
        }

        protected override void updateSimulationData(ActionData data, DietaryExposuresActionResult result) {
            data.DesiredIntakeModelType = result.DesiredIntakeModelType;
            data.SimulatedIndividualDays = result.SimulatedIndividualDays;
            data.DietaryIndividualDayIntakes = result.DietaryIndividualDayIntakes;
            data.DietaryObservedIndividualMeans = result.DietaryObservedIndividualMeans;
            data.DietaryModelAssistedIntakes = result.DietaryModelAssistedIntakes;
            data.DrillDownDietaryIndividualIntakes = result.IndividualModelAssistedIntakes;
            data.DietaryExposuresIntakeModel = result.IntakeModel;
            data.DietaryModelBasedIntakeResults = result.DietaryModelBasedIntakeResults;
            data.DietaryExposureUnit = result.DietaryExposureUnit;
            data.TdsReductionFactors = result.TdsReductionFactors ?? data.TdsReductionFactors;
            data.TdsReductionScenarioAnalysisFoods = result.TdsReductionScenarioAnalysisFoods ?? data.TdsReductionScenarioAnalysisFoods;
            //data.ExposureMatrix = result.ExposureMatrix;
        }

        protected override DietaryExposuresActionResult runUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            var settings = new DietaryExposuresModuleSettings(_project, true);

            var result = new DietaryExposuresActionResult();
            var uncertaintyFactorialResponses = new List<double>();
            var substances = data.ActiveSubstances;

            result.DietaryExposureUnit = data.DietaryExposureUnit;

            // Create residue generator
            var residueGeneratorFactory = new ResidueGeneratorFactory(settings);
            var residueGenerator = residueGeneratorFactory.Create(
                data.MonteCarloSubstanceSampleCollections?.ToDictionary(r => r.Food),
                data.ConcentrationModels,
                data.CumulativeConcentrationModels,
                data.CorrectedRelativePotencyFactors,
                data.MarginalOccurrencePatterns
            );

            // Unit variability model calculator
            var unitVariabilityCalculator = settings.ExposureType == ExposureType.Acute && settings.UseUnitVariability
                ? new UnitVariabilityCalculator(settings, data.UnitVariabilityDictionary)
                : null;

            // Create an individual day intakes pruner to prune individual day intakes
            IIndividualDayIntakePruner individualDayIntakePruner;
            if (settings.DietaryExposuresDetailsLevel == DietaryExposuresDetailsLevel.OnlyRiskDrivers
                && data.ScreeningResult?.ScreeningResultsPerFoodCompound != null) {
                individualDayIntakePruner = new ScreeningToAggregateIntakesPruner(
                    data.ScreeningResult?.ScreeningResultsPerFoodCompound,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities
                );
            } else if (settings.DietaryExposuresDetailsLevel == DietaryExposuresDetailsLevel.OmitFoodsAsEaten) {
                individualDayIntakePruner = new AggregateByFoodAsMeasuredPruner();
            } else {
                individualDayIntakePruner = new VoidPruner();
            }

            // Create intake calculator
            var intakeCalculatorFactory = new IntakeCalculatorFactory(settings);
            var intakeCalculator = intakeCalculatorFactory.Create(
                data.ProcessingFactorModels,
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
            var individualsRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.DE_DrawIndividuals));

            var populationGeneratorFactory = new PopulationGeneratorFactory(settings);
            var populationGenerator = populationGeneratorFactory.Create();
            var simulatedIndividualDays = populationGenerator
                .CreateSimulatedIndividualDays(
                    data.ModelledFoodConsumers,
                    data.ModelledFoodConsumerDays,
                    individualsRandomGenerator
                );
            result.SimulatedIndividualDays = simulatedIndividualDays;

            // Compute exposures of uncertainty run
            var uncertaintyDietaryIntakes = intakeCalculator
                .CalculateDietaryIntakes(
                    simulatedIndividualDays,
                    new ProgressState(progressReport.CancellationToken),
                    _project.MonteCarloSettings.RandomSeed // TODO: refactor (main seed currently splits up in different random sources in the calculator itself)
                );
            result.DietaryIndividualDayIntakes = uncertaintyDietaryIntakes;
            data.DietaryIndividualDayIntakes = uncertaintyDietaryIntakes;

            // Exposure imputation
            if (settings.ImputeExposureDistributions) {
                var exposurePerCompoundRecords = intakeCalculator
                    .ComputeExposurePerCompoundRecords(data.DietaryIndividualDayIntakes);
                var exposureImputationCalculator = new DietaryExposureImputationCalculator();
                var exposureImputationRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.DE_DrawImputedExposures));
                if (factorialSet.Contains(UncertaintySource.ImputeExposureDistributions)) {
                    uncertaintyDietaryIntakes = exposureImputationCalculator
                        .ImputeUncertaintyRun(
                            settings.ExposureType,
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
                            settings.ExposureType,
                            exposurePerCompoundRecords,
                            data.CompoundResidueCollections,
                            substances,
                            data.DietaryIndividualDayIntakes,
                            exposureImputationRandomGenerator
                        );
                }
            }

            if (substances.Count == 1 || data.CorrectedRelativePotencyFactors != null) {
                if (settings.ExposureType == ExposureType.Chronic) {
                    var factory = new IntakeModelFactory(
                        settings.FrequencyModelCalculationSettings,
                        settings.AmountModelCalculationSettings,
                        settings.ISUFModelCalculationSettings,
                        settings.NumberOfMonteCarloIterations,
                        settings.Intervals,
                        settings.ExtraPredictionLevels,
                        settings.Dispersion,
                        settings.VarianceRatio);
                    var simpleIndividualDayIntakesCalculator = new SimpleIndividualDayIntakesCalculator(
                        substances,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        settings.IsPerPerson,
                        null
                    );
                    var simpleIndividualDayIntakes = simpleIndividualDayIntakesCalculator
                        .Compute(uncertaintyDietaryIntakes);
                    var observedIndividualMeans = OIMCalculator
                        .CalculateObservedIndividualMeans(simpleIndividualDayIntakes);
                    if (settings.FirstModelThenAdd) {
                        var compositeIntakeModel = factory
                            .CreateCompositeIntakeModel(
                                uncertaintyDietaryIntakes,
                                data.ModelledFoods,
                                settings.IntakeModelsPerCategory
                            );
                        foreach (var subModel in compositeIntakeModel.PartialModels) {
                            var categoryIndividualDayIntakesCalculator = new SimpleIndividualDayIntakesCalculator(
                                substances,
                                data.CorrectedRelativePotencyFactors,
                                data.MembershipProbabilities,
                                settings.IsPerPerson,
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
                            settings.NumberOfMonteCarloIterations,
                            settings.FrequencyModelCalculationSettings.CovariateModelType,
                            settings.AmountModelCalculationSettings.CovariateModelType,
                            RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.DE_DrawModelBasedExposures),
                            RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.DE_DrawModelAssistedExposures)
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
                            settings.CovariateModelling,
                            settings.ExposureType,
                            data.DesiredIntakeModelType,
                            settings.TransformType
                        );
                        result.DesiredIntakeModelType = data.DesiredIntakeModelType;
                        intakeModel.CalculateParameters(simpleIndividualDayIntakes);
                        var usualIntakeCalculator = new UsualIntakesCalculator();
                        var intakeResults = usualIntakeCalculator.CalculateUsualIntakes(
                            intakeModel,
                            settings.ExposureType,
                            settings.CovariateModelling,
                            RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.DE_DrawModelBasedExposures),
                            RandomUtils.CreateSeed(_project.MonteCarloSettings.RandomSeed, (int)RandomSource.DE_DrawModelAssistedExposures)
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
                            .Select(c => c.TotalExposurePerMassUnit(data.CorrectedRelativePotencyFactors, data.MembershipProbabilities, settings.IsPerPerson))
                            .ToList();
                    } else {
                        uncertaintyFactorialResponses = result.DietaryIndividualDayIntakes
                            .Select(c => c.GetSubstanceTotalExposurePerMassUnit(data.ActiveSubstances.First(), settings.IsPerPerson))
                            .ToList();
                    }
                }
            }

            // Update factorial results
            result.FactorialResult = new DietaryExposuresFactorialResult() {
                Percentages = _project.OutputDetailSettings.SelectedPercentiles,
                Percentiles = uncertaintyFactorialResponses?
                    .PercentilesWithSamplingWeights(null, _project.OutputDetailSettings.SelectedPercentiles).ToList()
            };

            localProgress.Update(100);
            return result;
        }

        protected override void writeOutputData(IRawDataWriter rawDataWriter, ActionData data, DietaryExposuresActionResult result) {
            var outputWriter = new DietaryExposuresOutputWriter();
            outputWriter.WriteOutputData(_project, data, result, rawDataWriter);
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, DietaryExposuresActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new DietaryExposuresSummarizer();
            summarizer.SummarizeUncertain(_project, actionResult, data, header);
            localProgress.Update(100);
        }

        protected override void updateSimulationDataUncertain(ActionData data, DietaryExposuresActionResult result) {
            updateSimulationData(data, result);
        }

        protected override void writeOutputDataUncertain(IRawDataWriter rawDataWriter, ActionData data, DietaryExposuresActionResult result, int idBootstrap) {
            var outputWriter = new DietaryExposuresOutputWriter();
            outputWriter.UpdateOutputData(_project, rawDataWriter, data, result, idBootstrap);
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
                .Where(r => (r as DietaryExposuresActionComparisonData).DietaryExposureModels?.Any() ?? false)
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
