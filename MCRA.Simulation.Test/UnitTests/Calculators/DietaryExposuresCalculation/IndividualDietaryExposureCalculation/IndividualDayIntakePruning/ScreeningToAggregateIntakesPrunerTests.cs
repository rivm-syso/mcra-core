using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDayPruning;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinations;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Test.Mock.MockCalculatorSettings;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.DietaryExposuresCalculation {
    /// <summary>
    /// DietaryExposuresCalculation calculator
    /// </summary>
    [TestClass]
    public class ScreeningToAggregateIntakesPrunerTests {

        /// <summary>
        /// Screening aggragate intake pruner
        /// </summary>
        [TestMethod]
        public void ScreeningToAggregateIntakesPrunerTest1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(8);
            var substances = MockSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);

            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var simulatedIndividualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individualDays);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var foodConversionResults = MockFoodConversionsGenerator.Create(foodTranslations, substances);
            var consumptions = MockConsumptionsByModelledFoodGenerator
                .Create(foods, individualDays)
                .GroupBy(r => (r.Individual, r.Day));

            var consumptionsCache = new Dictionary<(Individual, string), List<ConsumptionsByModelledFood>>();
            foreach (var item in consumptions) {
                consumptionsCache[item.Key] = item.ToList();
            }

            var mu = 2;
            var sigma = 1;
            var useFraction = 0.25;
            var lor = 2;
            var sampleSize = 200;
            var concentrationModels = MockConcentrationsModelsGenerator.Create(
                foods: foods,
                substances: substances,
                modelType: ConcentrationModelType.Empirical,
                mu: mu,
                sigma: sigma,
                useFraction: useFraction,
                lor: lor,
                sampleSize: sampleSize
            );
            var compoundResidueCollections = concentrationModels.Select(c => c.Value.Residues).ToList();

            var substanceBasedResidueGeneratorSettings = new MockResidueGeneratorSettings() {
                IsSampleBased = false,
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByZero,
                UseOccurrencePatternsForResidueGeneration = false,
                TreatMissingOccurrencePatternsAsNotOccurring = false,
                ExposureType = ExposureType.Acute
            };
            var residueGenerator = new SubstanceBasedResidueGenerator(concentrationModels, null, substanceBasedResidueGeneratorSettings);

            var isSampleBased = false;
            var unitVariabilitySettings = new MockUnitVariabilityCalculatorSettings() {
                UseUnitVariability = false
            };

            var dietarySection = new AcuteDietaryExposureCalculator(
                activeSubstances: substances,
                consumptionsByFoodsAsMeasured: consumptionsCache,
                processingFactorModelCollection: null,
                individualDayIntakePruner: null,
                residueGenerator: residueGenerator,
                unitVariabilityCalculator: null,
                consumptionsByModelledFood: consumptionsCache.SelectMany(c => c.Value).ToList(),
                numberOfMonteCarloIterations: 10000,
                isSampleBased: isSampleBased,
                isCorrelation: false,
                isSingleSamplePerDay: false
            );
            var dietaryIndividualDayIntakes = dietarySection.CalculateDietaryIntakes(
                simulatedIndividualDays,
                new ProgressState(),
                seed
            );
            var section = new AcuteScreeningCalculator(95, 95, 0, false);

            var screeningResult = section.Calculate(foodConversionResults, individualDays, foodConsumptions, compoundResidueCollections, memberships, null);
            Assert.IsTrue(screeningResult.EffectiveCumulativeSelectionPercentage > 95);
            Assert.IsTrue(screeningResult.SelectedNumberOfSccRecords > 0);

            var individualDayIntakePruner = new ScreeningToAggregateIntakesPruner(screeningResult.ScreeningResultsPerFoodCompound, rpfs, memberships);
            var prunedResult = individualDayIntakePruner.Prune(dietaryIndividualDayIntakes.First());
        }
    }
}
