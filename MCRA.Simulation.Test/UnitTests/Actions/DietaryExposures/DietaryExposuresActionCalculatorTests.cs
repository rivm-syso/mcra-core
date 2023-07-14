using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.DietaryExposures;
using MCRA.Simulation.Calculators.IntakeModelling.IndividualAmountCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Actions {
    /// <summary>
    /// Runs the DietaryExposures action
    /// </summary>
    [TestClass]
    public class DietaryExposuresActionCalculatorTests : ActionCalculatorTestsBase {

        /// <summary>
        /// Runs the DietaryExposures action: acute, 
        /// project.ConcentrationModelSettings.IsSampleBased = true;
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// project.AssessmentSettings.Cumulative = true;
        /// project.MonteCarloSettings.NumberOfMonteCarloIterations = 10;
        /// </summary>
        [DataRow(false)]
        [DataRow(true)]
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestAcuteCumulativeSampleBased(
            bool imputeExposureDistributions
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(2);
            var individuals = MockIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var substances = MockSubstancesGenerator.Create(2);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances);

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var monteCarloSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = MockCompoundResidueCollectionsGenerator
                .Create(substances, monteCarloSubstanceSampleCollections);

            var data = new ActionData() {
                AllFoods = foods,
                ModelledFoodConsumers = individuals,
                ModelledFoodConsumerDays = individualDays,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ConcentrationModels = concentrationModels,
                ConsumptionsByModelledFood = consumptionsByModelledFood,
                MonteCarloSubstanceSampleCollections = monteCarloSubstanceSampleCollections.Values,
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                FoodsAsEaten = foodsAsEaten,
                CompoundResidueCollections = compoundResidueCollections
            };

            var project = new ProjectDto();
            project.ConcentrationModelSettings.IsSampleBased = true;
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.AssessmentSettings.Cumulative = true;
            project.MonteCarloSettings.NumberOfMonteCarloIterations = 10;
            project.DietaryIntakeCalculationSettings.ImputeExposureDistributions = imputeExposureDistributions;

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteCumulativeSampleBased,impute={imputeExposureDistributions}");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
        }

        /// <summary>
        /// Runs the DietaryExposures action: acute, 
        /// project.ConcentrationModelSettings.IsSampleBased = false;
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// project.AssessmentSettings.Cumulative = true;
        /// project.MonteCarloSettings.NumberOfMonteCarloIterations = 10;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestAcuteCumulativeSubstanceBased() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(2);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var individuals = MockIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var substances = MockSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );

            var data = new ActionData() {
                AllFoods = foods,
                ModelledFoodConsumers = individuals,
                ModelledFoodConsumerDays = individualDays,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ConcentrationModels = concentrationModels,
                ConsumptionsByModelledFood = consumptionsByModelledFood,
                MonteCarloSubstanceSampleCollections = activeSubstanceSampleCollections.Values,
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                FoodsAsEaten = foodsAsEaten
            };

            var project = new ProjectDto();
            project.ConcentrationModelSettings.IsSampleBased = false;
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.AssessmentSettings.Cumulative = true;
            project.MonteCarloSettings.NumberOfMonteCarloIterations = 10;

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteCumulativeSubstanceBased");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
        }


        /// <summary>
        /// Runs the DietaryExposures action: 
        /// project.DietaryIntakeCalculationSettings.ImputeExposureDistributions = true;
        /// project.ConcentrationModelSettings.IsSampleBased = false;
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// project.AssessmentSettings.Cumulative = true;
        /// project.MonteCarloSettings.NumberOfMonteCarloIterations = 10;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestAcuteSubstanceBasedImputeExposures() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(2);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var individuals = MockIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var substances = MockSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );
            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = MockCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

            var data = new ActionData() {
                AllFoods = foods,
                ModelledFoodConsumers = individuals,
                ModelledFoodConsumerDays = individualDays,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ConcentrationModels = concentrationModels,
                ConsumptionsByModelledFood = consumptionsByModelledFood,
                MonteCarloSubstanceSampleCollections = activeSubstanceSampleCollections.Values,
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                FoodsAsEaten = foodsAsEaten,
                CompoundResidueCollections = compoundResidueCollections
            };

            var project = new ProjectDto();
            project.DietaryIntakeCalculationSettings.ImputeExposureDistributions = true;
            project.ConcentrationModelSettings.IsSampleBased = false;
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.AssessmentSettings.Cumulative = true;
            project.MonteCarloSettings.NumberOfMonteCarloIterations = 10;
            project.DietaryIntakeCalculationSettings.VariabilityDiagnosticsAnalysis = true;
            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteSubstanceBasedImputeExposures");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
        }

        /// <summary>
        /// Runs the DietaryExposures action: 
        /// project.ConcentrationModelSettings.IsProcessing = true;
        /// project.ConcentrationModelSettings.IsSampleBased = false;
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// project.AssessmentSettings.Cumulative = true;
        /// project.MonteCarloSettings.NumberOfMonteCarloIterations = 10;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestAcuteCumulativeSubstanceBasedProcessing() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(2);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var individuals = MockIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var substances = MockSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = MockCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);
            var processingTypes = MockProcessingTypesGenerator.Create(modelledFoods.Count);
            var ix = 0;
            foreach (var food in modelledFoods) {
                food.ProcessingTypes = new List<ProcessingType> { processingTypes[ix] };
            }
            var processingFactorModels = MockProcessingFactorsGenerator
                .CreateProcessingFactorModelCollection(
                foods: modelledFoods,
                substances: substances,
                processingTypes: processingTypes,
                random: random,
                isProcessing: true,
                isDistribution: false,
                allowHigherThanOne: false,
                fractionMissing: 0
            );
            var data = new ActionData() {
                AllFoods = foods,
                ModelledFoodConsumers = individuals,
                ModelledFoodConsumerDays = individualDays,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ConcentrationModels = concentrationModels,
                ConsumptionsByModelledFood = consumptionsByModelledFood,
                MonteCarloSubstanceSampleCollections = activeSubstanceSampleCollections.Values,
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                FoodsAsEaten = foodsAsEaten,
                CompoundResidueCollections = compoundResidueCollections,
                ProcessingFactorModels = processingFactorModels,
                ProcessingTypes = processingTypes
            };

            var project = new ProjectDto();
            project.ConcentrationModelSettings.IsProcessing = true;
            project.ConcentrationModelSettings.IsSampleBased = false;
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.AssessmentSettings.Cumulative = true;
            project.MonteCarloSettings.NumberOfMonteCarloIterations = 10;

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteCumulativeSubstanceBasedProcessing");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
        }

        /// <summary>
        /// Runs the DietaryExposures action: 
        /// project.ConcentrationModelSettings.IsProcessing = true;
        /// project.UnitVariabilitySettings.UseUnitVariability = true;
        /// project.ConcentrationModelSettings.IsSampleBased = false;
        /// project.AssessmentSettings.ExposureType = ExposureType.Acute;
        /// project.AssessmentSettings.Cumulative = true;
        /// project.MonteCarloSettings.NumberOfMonteCarloIterations = 10;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestAcuteCumulativeSubstanceBasedProcessingUnitVariability() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(2);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var individuals = MockIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var substances = MockSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );
            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator
                .Create(
                    modelledFoods,
                    substances,
                    concentrationModels
                );
            var compoundResidueCollections = MockCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);
            var processingTypes = MockProcessingTypesGenerator.Create(3);
            var processingFactorModels = MockProcessingFactorsGenerator
                .CreateProcessingFactorModelCollection(
                foods: modelledFoods,
                substances: substances,
                processingTypes: processingTypes,
                random: random,
                isProcessing: true,
                isDistribution: true,
                allowHigherThanOne: false,
                fractionMissing: 0
            );
            var unitVariabilityDictionary = MockUnitVariabilityFactorsGenerator
                .Create(modelledFoods, substances, random);
            var data = new ActionData() {
                AllFoods = foods,
                ModelledFoodConsumers = individuals,
                ModelledFoodConsumerDays = individualDays,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ConcentrationModels = concentrationModels,
                ConsumptionsByModelledFood = consumptionsByModelledFood,
                MonteCarloSubstanceSampleCollections = activeSubstanceSampleCollections.Values,
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                FoodsAsEaten = foodsAsEaten,
                CompoundResidueCollections = compoundResidueCollections,
                ProcessingFactorModels = processingFactorModels,
                UnitVariabilityDictionary = unitVariabilityDictionary,
                ProcessingTypes = processingTypes
            };

            var project = new ProjectDto();
            project.ConcentrationModelSettings.IsProcessing = true;
            project.UnitVariabilitySettings.UseUnitVariability = true;
            project.ConcentrationModelSettings.IsSampleBased = false;
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.AssessmentSettings.Cumulative = true;
            project.MonteCarloSettings.NumberOfMonteCarloIterations = 10;

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteCumulativeSubstanceBasedProcessingUnitVariability");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.SimulatedIndividualDays);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);

        }

        /// <summary>
        /// Runs the DietaryExposures action: 
        /// project.DietaryIntakeCalculationSettings.ImputeExposureDistributions = true;
        /// project.ConcentrationModelSettings.IsProcessing = true;
        /// project.UnitVariabilitySettings.UseUnitVariability = true;
        /// project.ConcentrationModelSettings.IsSampleBased = false;
        /// project.AssessmentSettings.Cumulative = true;
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.OIM;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicOIM() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(2);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var individuals = MockIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var substances = MockSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = MockCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

            var data = new ActionData() {
                AllFoods = foods,
                ModelledFoodConsumers = individuals,
                ModelledFoodConsumerDays = individualDays,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ConcentrationModels = concentrationModels,
                MonteCarloSubstanceSampleCollections = activeSubstanceSampleCollections.Values,
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                FoodsAsEaten = foodsAsEaten,
                CompoundResidueCollections = compoundResidueCollections,
                ConsumptionsByModelledFood = consumptionsByModelledFood,
            };

            var project = new ProjectDto();
            project.DietaryIntakeCalculationSettings.ImputeExposureDistributions = true;
            project.ConcentrationModelSettings.IsProcessing = true;
            project.UnitVariabilitySettings.UseUnitVariability = true;
            project.ConcentrationModelSettings.IsSampleBased = false;
            project.AssessmentSettings.Cumulative = true;
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.IntakeModelSettings.IntakeModelType = IntakeModelType.OIM;
            project.DietaryIntakeCalculationSettings.VariabilityDiagnosticsAnalysis = true;

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicOIM");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.SimulatedIndividualDays);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
        }

        /// <summary>
        /// Runs the DietaryExposures action: 
        /// project.DietaryIntakeCalculationSettings.ImputeExposureDistributions = true;
        /// project.ConcentrationModelSettings.IsProcessing = true;
        /// project.UnitVariabilitySettings.UseUnitVariability = true;
        /// project.ConcentrationModelSettings.IsSampleBased = false;
        /// project.AssessmentSettings.Cumulative = true;
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.OIM;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicISUF() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(2);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var individuals = MockIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var substances = MockSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = MockCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

            var data = new ActionData() {
                AllFoods = foods,
                ModelledFoodConsumers = individuals,
                ModelledFoodConsumerDays = individualDays,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ConcentrationModels = concentrationModels,
                MonteCarloSubstanceSampleCollections = activeSubstanceSampleCollections.Values,
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                FoodsAsEaten = foodsAsEaten,
                CompoundResidueCollections = compoundResidueCollections,
                ConsumptionsByModelledFood = consumptionsByModelledFood,
            };

            var project = new ProjectDto();
            project.ConcentrationModelSettings.IsSampleBased = false;
            project.AssessmentSettings.Cumulative = true;
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.IntakeModelSettings.IntakeModelType = IntakeModelType.ISUF;

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicOIM");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.SimulatedIndividualDays);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
        }

        /// <summary>
        /// Runs the DietaryExposures action: 
        /// project.DietaryIntakeCalculationSettings.ImputeExposureDistributions = true;
        /// project.ConcentrationModelSettings.IsProcessing = true;
        /// project.UnitVariabilitySettings.UseUnitVariability = true;
        /// project.ConcentrationModelSettings.IsSampleBased = false;
        /// project.AssessmentSettings.Cumulative = true;
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.BBN;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicBBN() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(2);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var individuals = MockIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var substances = MockSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = MockCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

            var data = new ActionData() {
                AllFoods = foods,
                ModelledFoodConsumers = individuals,
                ModelledFoodConsumerDays = individualDays,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ConcentrationModels = concentrationModels,
                MonteCarloSubstanceSampleCollections = activeSubstanceSampleCollections.Values,
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                FoodsAsEaten = foodsAsEaten,
                CompoundResidueCollections = compoundResidueCollections,
                ConsumptionsByModelledFood = consumptionsByModelledFood,
            };

            var project = new ProjectDto();
            project.DietaryIntakeCalculationSettings.ImputeExposureDistributions = true;
            project.ConcentrationModelSettings.IsProcessing = true;
            project.UnitVariabilitySettings.UseUnitVariability = true;
            project.ConcentrationModelSettings.IsSampleBased = false;
            project.AssessmentSettings.Cumulative = true;
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.IntakeModelSettings.IntakeModelType = IntakeModelType.BBN;

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicBBN");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.SimulatedIndividualDays);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
            Assert.IsNotNull(data.DietaryModelAssistedIntakes);
            Assert.IsNotNull(data.DietaryExposuresIntakeModel);
        }

        /// <summary>
        /// Runs the DietaryExposures action: 
        /// project.ConcentrationModelSettings.IsSampleBased = true;
        /// project.AssessmentSettings.Cumulative = true;
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.OIM;
        /// project.IntakeModelSettings.FirstModelThenAdd = true;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicMTA() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var foods = MockFoodsGenerator.Create(4);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var individualDays = MockIndividualDaysGenerator.Create(4, 2, true, random, properties);
            var individuals = individualDays.Select(c => c.Individual).Distinct().ToList();
            var substances = MockSubstancesGenerator.Create(4);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, (ICollection<IndividualDay>)individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = MockCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

            var data = new ActionData() {
                AllFoods = foods,
                ModelledFoodConsumers = individuals,
                ModelledFoodConsumerDays = individualDays,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ConcentrationModels = concentrationModels,
                MonteCarloSubstanceSampleCollections = activeSubstanceSampleCollections.Values,
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                FoodsAsEaten = foodsAsEaten,
                CompoundResidueCollections = compoundResidueCollections,
                ConsumptionsByModelledFood = consumptionsByModelledFood,
            };

            // Create project
            var project = new ProjectDto();
            project.ConcentrationModelSettings.IsSampleBased = true;
            project.AssessmentSettings.Cumulative = true;
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.IntakeModelSettings.IntakeModelType = IntakeModelType.OIM;
            project.IntakeModelSettings.FirstModelThenAdd = true;
            project.IntakeModelSettings.IntakeModelsPerCategory = new List<IntakeModelPerCategoryDto>() {
                new IntakeModelPerCategoryDto() {
                    FoodsAsMeasured = modelledFoods.Take(2).Select(r => r.Code).ToList(),
                    ModelType = IntakeModelType.BBN,
                    TransformType = TransformType.Logarithmic
                },
                new IntakeModelPerCategoryDto() {
                    FoodsAsMeasured = modelledFoods.Skip(1).Take(1).Select(r => r.Code).ToList(),
                    ModelType = IntakeModelType.LNN0,
                    TransformType = TransformType.Logarithmic
                },
            };

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicMTA");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            header.SaveSummarySectionsRecursive(new CompositeProgressState());

            //var mtaHeader = header.GetXmlDataHeader(OutputConstants.MtaIntakeByFoodAsMeasuredSectionGuid);
            //var outputPath = Path.Combine(_reportOutputPath, GetType().Name, $"TestChronicMTA-MTA-data.xml");
            //mtaHeader.SaveXmlFile(header.SectionManager, outputPath);

            Assert.IsNotNull(data.SimulatedIndividualDays);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
            Assert.IsNotNull(data.DietaryModelAssistedIntakes);
            Assert.IsNotNull(data.DietaryExposuresIntakeModel);
        }

        /// <summary>
        /// Runs the DietaryExposures action: 
        /// project.ConcentrationModelSettings.IsSampleBased = true;
        /// project.AssessmentSettings.Cumulative = true;
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.LNN;
        /// project.IntakeModelSettings.CovariateModelling = true;
        /// project.OutputDetailSettings.SummarizeSimulatedData = true;
        /// project.OutputDetailSettings.IsDetailedOutput = true;
        /// project.IntakeModelSettings.CovariateModelling = true;
        /// project.FrequencyModelSettings.CovariateModelType = CovariateModelType.Constant;
        /// project.AmountModelSettings.MinDegreesOfFreedom = 1;
        /// project.AmountModelSettings.MaxDegreesOfFreedom = 1;
        /// project.AmountModelSettings.CovariateModelType = CovariateModelType.Covariable;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicLNN() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var foods = MockFoodsGenerator.Create(2);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var individualDays = MockIndividualDaysGenerator.Create(5, 2, true, random, properties);
            var individuals = individualDays.Select(c => c.Individual).Distinct().ToList();
            var substances = MockSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, (ICollection<IndividualDay>)individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = MockCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

            var data = new ActionData() {
                AllFoods = foods,
                ModelledFoodConsumers = individuals,
                ModelledFoodConsumerDays = individualDays,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ConcentrationModels = concentrationModels,
                MonteCarloSubstanceSampleCollections = activeSubstanceSampleCollections.Values,
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                FoodsAsEaten = foodsAsEaten,
                CompoundResidueCollections = compoundResidueCollections,
                ConsumptionsByModelledFood = consumptionsByModelledFood,
            };

            var project = new ProjectDto();
            project.ConcentrationModelSettings.IsSampleBased = true;
            project.AssessmentSettings.Cumulative = true;
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.IntakeModelSettings.IntakeModelType = IntakeModelType.LNN;
            project.IntakeModelSettings.CovariateModelling = true;
            project.OutputDetailSettings.SummarizeSimulatedData = true;
            project.OutputDetailSettings.IsDetailedOutput = true;
            project.IntakeModelSettings.CovariateModelling = true;
            project.FrequencyModelSettings.CovariateModelType = CovariateModelType.Constant;
            project.AmountModelSettings.MinDegreesOfFreedom = 1;
            project.AmountModelSettings.MaxDegreesOfFreedom = 1;
            project.AmountModelSettings.CovariateModelType = CovariateModelType.Covariable;

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicLNN");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.SimulatedIndividualDays);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
            Assert.IsNotNull(data.DietaryExposuresIntakeModel);
        }

        /// <summary>
        /// Runs the DietaryExposures action: chronic, LNN with covariates
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicLNN0() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var foods = MockFoodsGenerator.Create(2);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var individualDays = MockIndividualDaysGenerator.Create(5, 2, true, random, properties);
            var individuals = individualDays.Select(c => c.Individual).Distinct().ToList();
            var substances = MockSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, (ICollection<IndividualDay>)individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );
            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = MockCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

            var data = new ActionData() {
                AllFoods = foods,
                ModelledFoodConsumers = individuals,
                ModelledFoodConsumerDays = individualDays,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ConcentrationModels = concentrationModels,
                MonteCarloSubstanceSampleCollections = activeSubstanceSampleCollections.Values,
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                FoodsAsEaten = foodsAsEaten,
                CompoundResidueCollections = compoundResidueCollections,
                ConsumptionsByModelledFood = consumptionsByModelledFood,
            };

            var project = new ProjectDto();
            project.ConcentrationModelSettings.IsSampleBased = true;
            project.AssessmentSettings.Cumulative = true;
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.IntakeModelSettings.IntakeModelType = IntakeModelType.LNN0;
            project.IntakeModelSettings.CovariateModelling = true;
            project.OutputDetailSettings.SummarizeSimulatedData = true;
            project.OutputDetailSettings.IsDetailedOutput = true;
            project.IntakeModelSettings.CovariateModelling = true;
            project.FrequencyModelSettings.CovariateModelType = CovariateModelType.Constant;
            project.AmountModelSettings.MinDegreesOfFreedom = 1;
            project.AmountModelSettings.MaxDegreesOfFreedom = 1;
            project.AmountModelSettings.CovariateModelType = CovariateModelType.Covariable;

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicLNN0");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.SimulatedIndividualDays);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
            Assert.IsNotNull(data.DietaryModelAssistedIntakes);
            Assert.IsNotNull(data.DietaryExposuresIntakeModel);
        }

        /// <summary>
        /// Runs the DietaryExposures action: 
        /// project.ConcentrationModelSettings.IsSampleBased = true;
        /// project.AssessmentSettings.Cumulative = true;
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.LNN;
        /// project.IntakeModelSettings.CovariateModelling = true;
        /// project.OutputDetailSettings.SummarizeSimulatedData = true;
        /// project.OutputDetailSettings.IsDetailedOutput = true;
        /// project.IntakeModelSettings.CovariateModelling = true;
        /// project.FrequencyModelSettings.CovariateModelType = CovariateModelType.Constant;
        /// project.AmountModelSettings.MinDegreesOfFreedom = 1;
        /// project.AmountModelSettings.MaxDegreesOfFreedom = 1;
        /// project.AmountModelSettings.CovariateModelType = CovariateModelType.Covariable;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicLNN0Fallback() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = MockIndividualPropertiesGenerator.Create();
            var foods = MockFoodsGenerator.Create(2);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var individualDays = MockIndividualDaysGenerator.Create(5, 2, true, random, properties);
            var individuals = individualDays.Select(c => c.Individual).Distinct().ToList();
            var substances = MockSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, (ICollection<IndividualDay>)individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );
            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = MockCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

            var data = new ActionData() {
                AllFoods = foods,
                ModelledFoodConsumers = individuals,
                ModelledFoodConsumerDays = individualDays,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ConcentrationModels = concentrationModels,
                MonteCarloSubstanceSampleCollections = activeSubstanceSampleCollections.Values,
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                FoodsAsEaten = foodsAsEaten,
                CompoundResidueCollections = compoundResidueCollections,
                ConsumptionsByModelledFood = consumptionsByModelledFood,
            };

            var project = new ProjectDto();
            project.ConcentrationModelSettings.IsSampleBased = true;
            project.AssessmentSettings.Cumulative = true;
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.IntakeModelSettings.IntakeModelType = IntakeModelType.LNN;
            project.IntakeModelSettings.CovariateModelling = true;
            project.OutputDetailSettings.SummarizeSimulatedData = true;
            project.OutputDetailSettings.IsDetailedOutput = true;
            project.IntakeModelSettings.CovariateModelling = true;
            project.FrequencyModelSettings.CovariateModelType = CovariateModelType.Constant;
            project.AmountModelSettings.MinDegreesOfFreedom = 1;
            project.AmountModelSettings.MaxDegreesOfFreedom = 1;
            project.AmountModelSettings.CovariateModelType = CovariateModelType.Covariable;

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicLNN0Fallback");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.SimulatedIndividualDays);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
            Assert.IsNotNull(data.DietaryExposuresIntakeModel);
        }

        /// <summary>
        /// Runs the DietaryExposures action with substance dependent conversion paths: 
        /// project.ConcentrationModelSettings.IsSampleBased = false;
        /// project.AssessmentSettings.Cumulative = true;
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.OIM;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicOIMSubstanceDependent() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var modelledFoods = MockFoodsGenerator.MockFoods("A.16", "A.16.03", "A.16.03.001", "A.16.03.001.009");
            var foodsAsEaten = new List<Food> { modelledFoods[3], modelledFoods[2] };

            var foodTranslations = new List<FoodTranslation>();
            foreach (var foodAsEaten in foodsAsEaten) {
                foreach (var foodAsMeasured in modelledFoods) {
                    foodTranslations.Add(new FoodTranslation() { FoodFrom = foodAsEaten, FoodTo = foodAsMeasured, Proportion = 1 });
                }
            }

            var individuals = MockIndividualsGenerator.Create(10, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var substances = MockSubstancesGenerator.Create(foodTranslations.Count);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);

            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foodsAsEaten, individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .CreateSubstanceDependentConverionPaths(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = MockCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

            var data = new ActionData() {
                AllFoods = foodsAsEaten,
                ModelledFoodConsumers = individuals,
                ModelledFoodConsumerDays = individualDays,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ConcentrationModels = concentrationModels,
                MonteCarloSubstanceSampleCollections = activeSubstanceSampleCollections.Values,
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                FoodsAsEaten = foodsAsEaten,
                CompoundResidueCollections = compoundResidueCollections,
                ConsumptionsByModelledFood = consumptionsByModelledFood,
            };

            var project = new ProjectDto();
            project.ConcentrationModelSettings.IsSampleBased = false;
            project.AssessmentSettings.Cumulative = true;
            project.AssessmentSettings.ExposureType = ExposureType.Chronic;
            project.IntakeModelSettings.IntakeModelType = IntakeModelType.OIM;

            var calculator = new DietaryExposuresActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicOIMSubstanceDependent");

            Assert.IsNotNull(data.SimulatedIndividualDays);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
            var mean = data.DietaryObservedIndividualMeans.Average(c => c.DietaryIntakePerMassUnit);
            Assert.IsTrue(!double.IsNaN(mean));
            Assert.AreEqual(10, data.DietaryObservedIndividualMeans.Count);
        }

        /// <summary>
        /// Runs the DietaryExposures action with substance dependent conversion paths: 
        /// project.ConcentrationModelSettings.IsSampleBased = false;
        /// project.AssessmentSettings.Cumulative = true;
        /// project.AssessmentSettings.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.OIM;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestAcuteSubstanceDependent() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var modelledFoods = MockFoodsGenerator.MockFoods("A.16", "A.16.03", "A.16.03.001", "A.16.03.001.009");
            var foodsAsEaten = new List<Food> { modelledFoods[3], modelledFoods[2] };

            var foodTranslations = new List<FoodTranslation>();
            foreach (var foodAsEaten in foodsAsEaten) {
                foreach (var foodAsMeasured in modelledFoods) {
                    foodTranslations.Add(new FoodTranslation() { FoodFrom = foodAsEaten, FoodTo = foodAsMeasured, Proportion = 1 });
                }
            }

            var individuals = MockIndividualsGenerator.Create(10, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var substances = MockSubstancesGenerator.Create(foodTranslations.Count);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);

            var concentrationModels = MockConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foodsAsEaten, individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .CreateSubstanceDependentConverionPaths(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var activeSubstanceSampleCollections = MockSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = MockCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

            var data = new ActionData() {
                AllFoods = foodsAsEaten,
                ModelledFoodConsumers = individuals,
                ModelledFoodConsumerDays = individualDays,
                ReferenceSubstance = substances.First(),
                CorrectedRelativePotencyFactors = correctedRelativePotencyFactors,
                MembershipProbabilities = membershipProbabilities,
                ConcentrationModels = concentrationModels,
                MonteCarloSubstanceSampleCollections = activeSubstanceSampleCollections.Values,
                ActiveSubstances = substances,
                ModelledFoods = modelledFoods,
                FoodsAsEaten = foodsAsEaten,
                CompoundResidueCollections = compoundResidueCollections,
                ConsumptionsByModelledFood = consumptionsByModelledFood,
            };

            var project = new ProjectDto();
            project.ConcentrationModelSettings.IsSampleBased = false;
            project.AssessmentSettings.Cumulative = true;
            project.AssessmentSettings.ExposureType = ExposureType.Acute;
            project.MonteCarloSettings.NumberOfMonteCarloIterations = 100;

            var calculator = new DietaryExposuresActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteSubstanceDependent");

            Assert.IsNotNull(data.SimulatedIndividualDays);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            var simpleIndividualDayIntakesCalculator = new SimpleIndividualDayIntakesCalculator(
                    data.ActiveSubstances,
                    data.CorrectedRelativePotencyFactors,
                    data.MembershipProbabilities,
                    false,
                    null
                );
            var simpleIndividualDayIntakes = simpleIndividualDayIntakesCalculator
                .Compute(data.DietaryIndividualDayIntakes);
            var mean = simpleIndividualDayIntakes.Average(c => c.Amount);
            Assert.IsTrue(!double.IsNaN(mean));
            Assert.AreEqual(100, data.DietaryIndividualDayIntakes.Count);
        }
    }
}