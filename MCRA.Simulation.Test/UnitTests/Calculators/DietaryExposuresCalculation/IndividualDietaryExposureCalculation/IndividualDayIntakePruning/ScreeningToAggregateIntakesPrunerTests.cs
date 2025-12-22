using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDayPruning;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinations;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;

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
            var foods = FakeFoodsGenerator.Create(8);
            var substances = FakeSubstancesGenerator.Create(3);
            var rpfs = substances.ToDictionary(r => r, r => 1d);
            var memberships = substances.ToDictionary(r => r, r => 1d);

            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var simulatedIndividualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individualDays);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var foodConversionResults = FakeFoodConversionsGenerator.Create(foodTranslations, substances);
            var consumptions = FakeConsumptionsByModelledFoodGenerator
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
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(
                foods: foods,
                substances: substances,
                modelType: ConcentrationModelType.Empirical,
                mu: mu,
                sigma: sigma,
                useFraction: useFraction,
                lor: lor,
                sampleSize: sampleSize
            );
            var compoundResidueCollections = concentrationModels.Select(c => new CompoundResidueCollection(c.Value.Residues) {
                 Food = c.Key.Item1,
                 Compound = c.Key.Item2
            }).ToList();

            var residueGenerator = new SubstanceBasedResidueGenerator(
                concentrationModels,
                null,
                useOccurrencePatternsForResidueGeneration: false,
                treatMissingOccurrencePatternsAsNotOccurring: false,
                nonDetectsHandlingMethod: NonDetectsHandlingMethod.ReplaceByZero
            );

            var dietarySection = new AcuteDietaryExposureCalculator(
                activeSubstances: substances,
                consumptionsByFoodsAsMeasured: consumptionsCache,
                processingFactorProvider: null,
                individualDayIntakePruner: null,
                residueGenerator: residueGenerator,
                unitVariabilityCalculator: null,
                consumptionsByModelledFood: consumptionsCache.SelectMany(c => c.Value).ToList(),
                numberOfMonteCarloIterations: 10000,
                isSampleBased: false,
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
            Assert.IsGreaterThan(95, screeningResult.EffectiveCumulativeSelectionPercentage);
            Assert.IsGreaterThan(0, screeningResult.SelectedNumberOfSccRecords);

            var individualDayIntakePruner = new ScreeningToAggregateIntakesPruner(screeningResult.ScreeningResultsPerFoodCompound, rpfs, memberships);
            var prunedResult = individualDayIntakePruner.Prune(dietaryIndividualDayIntakes.First());
        }
    }
}
