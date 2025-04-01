using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.DietaryExposuresCalculation {

    /// <summary>
    /// DietaryExposuresCalculation calculator
    /// </summary>
    [TestClass]
    public class AcuteDietaryExposureCalculatorTests {

        /// <summary>
        /// AcuteDietaryExposureCalculator: IsSampleBased = false
        /// </summary>
        [TestMethod]
        public void AcuteDietaryExposureCalculator_TestNotSampleBased() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var simulatedIndividualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individualDays);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(foods, individualDays)
                .GroupBy(r => (r.Individual, r.Day))
                .ToDictionary(r => r.Key, r => r.ToList());

            var isSampleBased = false;
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(foods, substances);
            var residueGenerator = new SubstanceBasedResidueGenerator(
                concentrationModels,
                null,
                useOccurrencePatternsForResidueGeneration: false,
                treatMissingOccurrencePatternsAsNotOccurring: false,
                nonDetectsHandlingMethod: NonDetectsHandlingMethod.ReplaceByZero
            );
            var calculator = new AcuteDietaryExposureCalculator(
                activeSubstances: substances,
                consumptionsByFoodsAsMeasured: consumptionsByModelledFood,
                processingFactorProvider: null,
                individualDayIntakePruner: null,
                residueGenerator: residueGenerator,
                unitVariabilityCalculator: null,
                consumptionsByModelledFood: consumptionsByModelledFood.SelectMany(c => c.Value).ToList(),
                numberOfMonteCarloIterations: 10000,
                isSampleBased: isSampleBased,
                isCorrelation: true,
                isSingleSamplePerDay: false
            );

            var dietaryIndividualDayIntakes = calculator
                .CalculateDietaryIntakes(simulatedIndividualDays, new ProgressState(), seed);

            var totalExposureModelledFoods = dietaryIndividualDayIntakes
                .SelectMany(c => c.GetModelledFoodTotalExposures(rpfs, memberships, isPerPerson: false))
                .Sum(c => c.Value.Exposure);

            var totalExposure = dietaryIndividualDayIntakes.Sum(c => c.TotalExposurePerMassUnit(rpfs, memberships, isPerPerson: false));
            Assert.IsTrue(!double.IsNaN(totalExposure));
            Assert.AreEqual(totalExposure, totalExposureModelledFoods, 1e-4);

            var exposurePerCompound = calculator.ComputeExposurePerCompoundRecords(dietaryIndividualDayIntakes);
            var totalExposurePerCompound = exposurePerCompound.SelectMany(c => c.Value).Sum(c => c.ExposurePerBodyWeight);
            Assert.IsTrue(!double.IsNaN(totalExposurePerCompound));
        }

        /// <summary>
        /// AcuteDietaryExposureCalculator: IsSampleBased = true
        /// </summary>
        [TestMethod]
        public void AcuteDietaryExposureCalculator_TestSampleBased() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);

            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var simulatedIndividualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individualDays);
            var consumptionsCache = new Dictionary<(Individual, string), List<ConsumptionsByModelledFood>>();
            var consumptions = FakeConsumptionsByModelledFoodGenerator
                .Create(foods, individualDays)
                .GroupBy(r => (r.Individual, r.Day));
            foreach (var item in consumptions) {
                consumptionsCache[item.Key] = item.ToList();
            }

            var isSampleBased = true;
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(foods, substances);
            var sampleCompoundCollections = FakeSampleCompoundCollectionsGenerator.Create(foods, substances, concentrationModels);
            var residueGenerator = new SampleBasedResidueGenerator(sampleCompoundCollections);

            var calculator = new AcuteDietaryExposureCalculator(
                substances,
                consumptionsCache,
                null,
                null,
                residueGenerator,
                null,
                consumptionsCache.SelectMany(c => c.Value).ToList(),
                10000,
                isSampleBased,
                false,
                false
            );

            var dietaryIndividualDayIntakes = calculator
                .CalculateDietaryIntakes(simulatedIndividualDays, new ProgressState(), seed);
            var totalExposure = dietaryIndividualDayIntakes.Sum(c => c.TotalExposurePerMassUnit(rpfs, memberships, false));
            Assert.IsTrue(!double.IsNaN(totalExposure));
        }
    }
}
