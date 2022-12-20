using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Test.Mock.MockCalculatorSettings;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

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
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var simulatedIndividualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individualDays);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .Create(foods, individualDays)
                .GroupBy(r => (r.Individual, r.Day))
                .ToDictionary(r => r.Key, r => r.ToList());

            var isSampleBased = false;
            var concentrationModels = MockConcentrationsModelsGenerator.Create(foods, substances);
            var substanceBasedResidueGeneratorSettings = new MockResidueGeneratorSettings() {
                IsSampleBased = false,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByZero,
                UseOccurrencePatternsForResidueGeneration = false,
                TreatMissingOccurrencePatternsAsNotOccurring = false,
                ExposureType = ExposureType.Acute
            };
            var residueGenerator = new SubstanceBasedResidueGenerator(concentrationModels, null, substanceBasedResidueGeneratorSettings);
            var calculator = new AcuteDietaryExposureCalculator(
                activeSubstances: substances,
                consumptionsByFoodsAsMeasured: consumptionsByModelledFood,
                processingFactorModelCollection: null,
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
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);

            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var simulatedIndividualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individualDays);
            var consumptionsCache = new Dictionary<(Individual, string), List<ConsumptionsByModelledFood>>();
            var consumptions = MockConsumptionsByModelledFoodGenerator
                .Create(foods, individualDays)
                .GroupBy(r => (r.Individual, r.Day));
            foreach (var item in consumptions) {
                consumptionsCache[item.Key] = item.ToList();
            }

            var isSampleBased = true;
            var concentrationModels = MockConcentrationsModelsGenerator.Create(foods, substances);
            var sampleCompoundCollections = MockSampleCompoundCollectionsGenerator.Create(foods, substances, concentrationModels);
            var residueGenerator = new SampleBasedResidueGenerator(sampleCompoundCollections?.ToDictionary(r => r.Food));

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
