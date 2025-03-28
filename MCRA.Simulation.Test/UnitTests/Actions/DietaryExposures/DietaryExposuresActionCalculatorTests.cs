﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions.DietaryExposures;
using MCRA.Simulation.Calculators.IntakeModelling.IndividualAmountCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
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
        /// config.IsSampleBased = true;
        /// config.ExposureType = ExposureType.Acute;
        /// config.Cumulative = true;
        /// config.NumberOfMonteCarloIterations = 10;
        /// </summary>
        [DataRow(false)]
        [DataRow(true)]
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestAcuteCumulativeSampleBased(
            bool imputeExposureDistributions
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(2);
            var individuals = FakeIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var substances = FakeSubstancesGenerator.Create(2);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var monteCarloSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator
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

            var config = new DietaryExposuresModuleConfig {
                IsSampleBased = true,
                ExposureType = ExposureType.Acute,
                Cumulative = true,
                NumberOfMonteCarloIterations = 10,
                ImputeExposureDistributions = imputeExposureDistributions
            };
            var project = new ProjectDto(config);

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteCumulativeSampleBased,impute={imputeExposureDistributions}");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
        }

        /// <summary>
        /// Runs the DietaryExposures action: acute,
        /// config.IsSampleBased = false;
        /// config.ExposureType = ExposureType.Acute;
        /// config.Cumulative = true;
        /// config.NumberOfMonteCarloIterations = 10;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestAcuteCumulativeSubstanceBased() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(2);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var individuals = FakeIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var substances = FakeSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
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
            var config = project.DietaryExposuresSettings;
            config.IsSampleBased = false;
            config.ExposureType = ExposureType.Acute;
            config.Cumulative = true;
            config.NumberOfMonteCarloIterations = 10;

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteCumulativeSubstanceBased");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
        }


        /// <summary>
        /// Runs the DietaryExposures action:
        /// config.ImputeExposureDistributions = true;
        /// config.IsSampleBased = false;
        /// config.ExposureType = ExposureType.Acute;
        /// config.Cumulative = true;
        /// config.NumberOfMonteCarloIterations = 10;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestAcuteSubstanceBasedImputeExposures() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(2);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var individuals = FakeIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var substances = FakeSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );
            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

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
            var config = project.DietaryExposuresSettings;
            config.ImputeExposureDistributions = true;
            config.IsSampleBased = false;
            config.ExposureType = ExposureType.Acute;
            config.Cumulative = true;
            config.NumberOfMonteCarloIterations = 10;
            config.VariabilityDiagnosticsAnalysis = true;
            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteSubstanceBasedImputeExposures");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
        }

        /// <summary>
        /// Runs the DietaryExposures action:
        /// config.IsProcessing = true;
        /// config.IsSampleBased = false;
        /// config.ExposureType = ExposureType.Acute;
        /// config.Cumulative = true;
        /// config.NumberOfMonteCarloIterations = 10;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestAcuteCumulativeSubstanceBasedProcessing() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(2);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var individuals = FakeIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var substances = FakeSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);
            var processingTypes = FakeProcessingTypesGenerator.Create(modelledFoods.Count);
            var ix = 0;
            foreach (var food in modelledFoods) {
                food.ProcessingTypes = [processingTypes[ix]];
            }
            var processingFactorModels = FakeProcessingFactorsGenerator
                .CreateProcessingFactorModelCollection(
                foods: modelledFoods,
                substances: substances,
                processingTypes: processingTypes,
                random: random,
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
            var config = project.DietaryExposuresSettings;
            config.IsProcessing = true;
            config.IsSampleBased = false;
            config.ExposureType = ExposureType.Acute;
            config.Cumulative = true;
            config.NumberOfMonteCarloIterations = 10;

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteCumulativeSubstanceBasedProcessing");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
        }

        /// <summary>
        /// Runs the DietaryExposures action:
        /// config.UseUnitVariability = true;
        /// config.IsSampleBased = false;
        /// config.ExposureType = ExposureType.Acute;
        /// config.Cumulative = true;
        /// config.NumberOfMonteCarloIterations = 10;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestAcuteCumulativeSubstanceBasedProcessingUnitVariability() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(2);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var individuals = FakeIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var substances = FakeSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );
            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator
                .Create(
                    modelledFoods,
                    substances,
                    concentrationModels
                );
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);
            var processingTypes = FakeProcessingTypesGenerator.Create(3);
            var processingFactorModels = FakeProcessingFactorsGenerator
                .CreateProcessingFactorModelCollection(
                foods: modelledFoods,
                substances: substances,
                processingTypes: processingTypes,
                random: random,
                isDistribution: true,
                allowHigherThanOne: false,
                fractionMissing: 0
            );
            var unitVariabilityDictionary = FakeUnitVariabilityFactorsGenerator
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
            var config = project.DietaryExposuresSettings;
            config.IsProcessing = true;
            config.UseUnitVariability = true;
            config.IsSampleBased = false;
            config.ExposureType = ExposureType.Acute;
            config.Cumulative = true;
            config.NumberOfMonteCarloIterations = 10;

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteCumulativeSubstanceBasedProcessingUnitVariability");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
        }

        /// <summary>
        /// Runs the DietaryExposures action:
        /// config.ImputeExposureDistributions = true;
        /// config.UseUnitVariability = true;
        /// config.IsSampleBased = false;
        /// config.Cumulative = true;
        /// config.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.OIM;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicOIM() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(2);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var individuals = FakeIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var substances = FakeSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

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

            var config = new DietaryExposuresModuleConfig {
                ImputeExposureDistributions = true,
                UseUnitVariability = true,
                IsSampleBased = false,
                Cumulative = true,
                ExposureType = ExposureType.Chronic,
                IntakeModelType = IntakeModelType.OIM,
                VariabilityDiagnosticsAnalysis = true
            };
            var project = new ProjectDto(config);

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicOIM");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
        }

        /// <summary>
        /// Runs the DietaryExposures action:
        /// config.ImputeExposureDistributions = true;
        /// config.IsProcessing = true;
        /// config.UseUnitVariability = true;
        /// config.IsSampleBased = false;
        /// config.Cumulative = true;
        /// config.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.OIM;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicISUF() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(2);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var individuals = FakeIndividualsGenerator.Create(10, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var substances = FakeSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

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

            var config = new DietaryExposuresModuleConfig {
                IsSampleBased = false,
                Cumulative = true,
                ExposureType = ExposureType.Chronic,
                IntakeModelType = IntakeModelType.ISUF
            };
            var project = new ProjectDto(config);

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicOIM");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
        }

        /// <summary>
        /// Runs the DietaryExposures action:
        /// config.ImputeExposureDistributions = true;
        /// config.IsProcessing = true;
        /// config.UseUnitVariability = true;
        /// config.IsSampleBased = false;
        /// config.Cumulative = true;
        /// config.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.BBN;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicBBN() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(2);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var individuals = FakeIndividualsGenerator.Create(2, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var substances = FakeSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

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

            var config = new DietaryExposuresModuleConfig {
                ImputeExposureDistributions = true,
                UseUnitVariability = true,
                IsSampleBased = false,
                Cumulative = true,
                ExposureType = ExposureType.Chronic,
                IntakeModelType = IntakeModelType.BBN
            };
            var project = new ProjectDto(config);

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicBBN");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
            Assert.IsNotNull(data.DietaryModelAssistedIntakes);
            Assert.IsNotNull(data.DietaryExposuresIntakeModel);
        }

        /// <summary>
        /// Runs the DietaryExposures action:
        /// config.IsSampleBased = true;
        /// config.Cumulative = true;
        /// config.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.OIM;
        /// project.IntakeModelSettings.IntakeFirstModelThenAdd = true;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicMTA() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var foods = FakeFoodsGenerator.Create(4);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var individualDays = FakeIndividualDaysGenerator.Create(4, 2, true, random, properties);
            var individuals = individualDays.Select(c => c.Individual).Distinct().ToList();
            var substances = FakeSubstancesGenerator.Create(4);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

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
            var config = new DietaryExposuresModuleConfig {
                IsSampleBased = true,
                Cumulative = true,
                ExposureType = ExposureType.Chronic,
                IntakeModelType = IntakeModelType.OIM,
                IntakeFirstModelThenAdd = true,
                IntakeModelsPerCategory = [
                new () {
                    FoodsAsMeasured = modelledFoods.Take(2).Select(r => r.Code).ToList(),
                    ModelType = IntakeModelType.BBN,
                    TransformType = TransformType.Logarithmic
                },
                new () {
                    FoodsAsMeasured = modelledFoods.Skip(1).Take(1).Select(r => r.Code).ToList(),
                    ModelType = IntakeModelType.LNN0,
                    TransformType = TransformType.Logarithmic
                },
            ]
            };
            var project = new ProjectDto(config);

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicMTA");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            header.SaveSummarySectionsRecursive(new CompositeProgressState());

            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
            Assert.IsNotNull(data.DietaryModelAssistedIntakes);
            Assert.IsNotNull(data.DietaryExposuresIntakeModel);
        }

        /// <summary>
        /// Runs the DietaryExposures action:
        /// config.IsSampleBased = true;
        /// config.Cumulative = true;
        /// config.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.LNN;
        /// project.IntakeModelSettings.IntakeCovariateModelling = true;
        /// project.OutputDetailSettings.IsDetailedOutput = true;
        /// project.IntakeModelSettings.IntakeCovariateModelling = true;
        /// project.FrequencyModelSettings.CovariateModelType = CovariateModelType.Constant;
        /// project.AmountModelSettings.MinDegreesOfFreedom = 1;
        /// project.AmountModelSettings.MaxDegreesOfFreedom = 1;
        /// project.AmountModelSettings.CovariateModelType = CovariateModelType.Covariable;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicLNN() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var foods = FakeFoodsGenerator.Create(2);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var individualDays = FakeIndividualDaysGenerator.Create(5, 2, true, random, properties);
            var individuals = individualDays.Select(c => c.Individual).Distinct().ToList();
            var substances = FakeSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

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

            var config = new DietaryExposuresModuleConfig {
                IsSampleBased = true,
                Cumulative = true,
                ExposureType = ExposureType.Chronic,
                IntakeModelType = IntakeModelType.LNN,
                IsDetailedOutput = true,
                IntakeCovariateModelling = true,
                FrequencyModelCovariateModelType = CovariateModelType.Constant,
                AmountModelMinDegreesOfFreedom = 1,
                AmountModelMaxDegreesOfFreedom = 1,
                AmountModelCovariateModelType = CovariateModelType.Covariable
            };
            var project = new ProjectDto(config);

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicLNN");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
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
            var properties = FakeIndividualPropertiesGenerator.Create();
            var foods = FakeFoodsGenerator.Create(2);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var individualDays = FakeIndividualDaysGenerator.Create(5, 2, true, random, properties);
            var individuals = individualDays.Select(c => c.Individual).Distinct().ToList();
            var substances = FakeSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );
            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

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

            var config = new DietaryExposuresModuleConfig {
                IsSampleBased = true,
                Cumulative = true,
                ExposureType = ExposureType.Chronic,
                IntakeModelType = IntakeModelType.LNN0,
                IntakeCovariateModelling = true,
                IsDetailedOutput = true,
                FrequencyModelCovariateModelType = CovariateModelType.Constant,
                AmountModelMinDegreesOfFreedom = 1,
                AmountModelMaxDegreesOfFreedom = 1,
                AmountModelCovariateModelType = CovariateModelType.Covariable
            };
            var project = new ProjectDto(config);

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicLNN0");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
            Assert.IsNotNull(data.DietaryModelAssistedIntakes);
            Assert.IsNotNull(data.DietaryExposuresIntakeModel);
        }

        /// <summary>
        /// Runs the DietaryExposures action:
        /// config.IsSampleBased = true;
        /// config.Cumulative = true;
        /// config.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.LNN;
        /// project.IntakeModelSettings.IntakeCovariateModelling = true;
        /// project.OutputDetailSettings.IsDetailedOutput = true;
        /// project.IntakeModelSettings.IntakeCovariateModelling = true;
        /// project.FrequencyModelSettings.CovariateModelType = CovariateModelType.Constant;
        /// project.AmountModelSettings.MinDegreesOfFreedom = 1;
        /// project.AmountModelSettings.MaxDegreesOfFreedom = 1;
        /// project.AmountModelSettings.CovariateModelType = CovariateModelType.Covariable;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicLNN0Fallback() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var foods = FakeFoodsGenerator.Create(2);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var individualDays = FakeIndividualDaysGenerator.Create(5, 2, true, random, properties);
            var individuals = individualDays.Select(c => c.Individual).Distinct().ToList();
            var substances = FakeSubstancesGenerator.Create(2);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );
            var foodsAsEaten = foodConsumptions.Select(c => c.Food).Distinct().ToList();
            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

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

            var config = new DietaryExposuresModuleConfig {
                IsSampleBased = true,
                Cumulative = true,
                ExposureType = ExposureType.Chronic,
                IntakeModelType = IntakeModelType.LNN,
                IsDetailedOutput = true,
                IntakeCovariateModelling = true,
                FrequencyModelCovariateModelType = CovariateModelType.Constant,
                AmountModelMinDegreesOfFreedom = 1,
                AmountModelMaxDegreesOfFreedom = 1,
                AmountModelCovariateModelType = CovariateModelType.Covariable
            };
            var project = new ProjectDto(config);

            var calculator = new DietaryExposuresActionCalculator(project);
            var (header, _) = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicLNN0Fallback");

            var factorialSet = new UncertaintyFactorialSet(UncertaintySource.Concentrations, UncertaintySource.Individuals, UncertaintySource.Processing);
            var uncertaintySourceGenerators = factorialSet.UncertaintySources.ToDictionary(r => r, r => random as IRandom);
            TestRunUpdateSummarizeUncertainty(calculator, data, header, random, factorialSet, uncertaintySourceGenerators);
            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
            Assert.IsNotNull(data.DietaryExposuresIntakeModel);
        }

        /// <summary>
        /// Runs the DietaryExposures action with substance dependent conversion paths:
        /// config.IsSampleBased = false;
        /// config.Cumulative = true;
        /// config.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.OIM;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestChronicOIMSubstanceDependent() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var modelledFoods = FakeFoodsGenerator.MockFoods("A.16", "A.16.03", "A.16.03.001", "A.16.03.001.009");
            var foodsAsEaten = new List<Food> { modelledFoods[3], modelledFoods[2] };

            var foodTranslations = new List<FoodTranslation>();
            foreach (var foodAsEaten in foodsAsEaten) {
                foreach (var foodAsMeasured in modelledFoods) {
                    foodTranslations.Add(new FoodTranslation() { FoodFrom = foodAsEaten, FoodTo = foodAsMeasured, Proportion = 1 });
                }
            }

            var individuals = FakeIndividualsGenerator.Create(10, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var substances = FakeSubstancesGenerator.Create(foodTranslations.Count);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);

            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foodsAsEaten, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .CreateSubstanceDependentConverionPaths(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

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

            var config = new DietaryExposuresModuleConfig {
                IsSampleBased = false,
                Cumulative = true,
                ExposureType = ExposureType.Chronic,
                IntakeModelType = IntakeModelType.OIM
            };
            var project = new ProjectDto(config);

            var calculator = new DietaryExposuresActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestChronicOIMSubstanceDependent");

            Assert.IsNotNull(data.DietaryIndividualDayIntakes);
            Assert.IsNotNull(data.DietaryExposureUnit);
            Assert.IsNotNull(data.DietaryObservedIndividualMeans);
            var mean = data.DietaryObservedIndividualMeans.Average(c => c.DietaryIntakePerMassUnit);
            Assert.IsTrue(!double.IsNaN(mean));
            Assert.AreEqual(10, data.DietaryObservedIndividualMeans.Count);
        }

        /// <summary>
        /// Runs the DietaryExposures action with substance dependent conversion paths:
        /// config.IsSampleBased = false;
        /// config.Cumulative = true;
        /// config.ExposureType = ExposureType.Chronic;
        /// project.IntakeModelSettings.IntakeModelType = IntakeModelType.OIM;
        /// </summary>
        [TestMethod]
        public void DietaryExposuresActionCalculator_TestAcuteSubstanceDependent() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var modelledFoods = FakeFoodsGenerator.MockFoods("A.16", "A.16.03", "A.16.03.001", "A.16.03.001.009");
            var foodsAsEaten = new List<Food> { modelledFoods[3], modelledFoods[2] };

            var foodTranslations = new List<FoodTranslation>();
            foreach (var foodAsEaten in foodsAsEaten) {
                foreach (var foodAsMeasured in modelledFoods) {
                    foodTranslations.Add(new FoodTranslation() { FoodFrom = foodAsEaten, FoodTo = foodAsMeasured, Proportion = 1 });
                }
            }

            var individuals = FakeIndividualsGenerator.Create(10, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var substances = FakeSubstancesGenerator.Create(foodTranslations.Count);
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var membershipProbabilities = substances.ToDictionary(c => c, c => 1d);

            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foodsAsEaten, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .CreateSubstanceDependentConverionPaths(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections);

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

            var config = new DietaryExposuresModuleConfig {
                IsSampleBased = false,
                Cumulative = true,
                ExposureType = ExposureType.Acute,
                NumberOfMonteCarloIterations = 100
            };
            var project = new ProjectDto(config);
            var calculator = new DietaryExposuresActionCalculator(project);
            var header = TestRunUpdateSummarizeNominal(project, calculator, data, $"TestAcuteSubstanceDependent");

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