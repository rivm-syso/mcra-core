using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Test.Mock.MockCalculatorSettings;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ResidueGeneration {
    /// <summary>
    /// ResidueGeneratorFactory  calculator
    /// </summary>
    [TestClass]
    public class ResidueGeneratorFactoryModelsTests {
        /// <summary>
        /// Test factory models
        /// </summary>
        [TestMethod]
        public void ResidueGeneratorFactory_MeanConcentrationResidueGeneratorTests() {
            var project = new ProjectDto();
            var concentrationModels = new Dictionary<(Food, Compound), ConcentrationModel>();
            var factory = new MeanConcentrationResidueGenerator(concentrationModels);
            Assert.IsNotNull(factory);
        }

        /// <summary>
        /// Test empty sets
        /// </summary>
        [TestMethod]
        public void ResidueGeneratorFactory_EquivalentsModelResidueGeneratorTests() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(4);
            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var substances = FakeSubstancesGenerator.Create(5);
            var modelledFoods = foodTranslations.Select(c => c.FoodTo).Distinct().ToList();
            var correctedRelativePotencyFactors = substances.ToDictionary(c => c, c => 1d);
            var concentrationModels = FakeConcentrationsModelsGenerator.Create(modelledFoods, substances);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var activeSubstanceSampleCollections = FakeSampleCompoundCollectionsGenerator.Create(
                modelledFoods,
                substances,
                concentrationModels
            );
            var cumulativeSubstance = substances.First();
            var concentrationModelTypesPerFoodSubstance = new List<ConcentrationModelTypeFoodSubstance>();
            foreach (var food in modelledFoods) {
                var dto = new ConcentrationModelTypeFoodSubstance() {
                    SubstanceCode = cumulativeSubstance.Code,
                    FoodCode = food.Code,
                    ModelType = ConcentrationModelType.Empirical
                };
                concentrationModelTypesPerFoodSubstance.Add(dto);
            }
            var settings = new MockConcentrationModelCalculationSettings() {
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                DefaultConcentrationModel = ConcentrationModelType.Empirical,
                ConcentrationModelTypesPerFoodCompound = concentrationModelTypesPerFoodSubstance,
                IsFallbackMrl = false,
                FractionOfMrl = 0.1,
                FractionOfLor = 0.1
            };
            var cumulativeConcentrationModelsCalculator = new CumulativeConcentrationModelsBuilder(settings);
            var compoundResidueCollections = FakeCompoundResidueCollectionsGenerator.Create(substances, activeSubstanceSampleCollections)
                .Where(c => c.Key.Substance == substances.First());

            var cumulativeConcentrationModels = cumulativeConcentrationModelsCalculator.Create(
                modelledFoods,
                compoundResidueCollections.ToDictionary(c => c.Key.Food, c => c.Value),
                cumulativeSubstance,
                ConcentrationUnit.mgPerKg
            );
            var factory = new EquivalentsModelResidueGenerator(
                correctedRelativePotencyFactors,
                cumulativeConcentrationModels,
                activeSubstanceSampleCollections
                //NonDetectsHandlingMethod.ReplaceByLOD
            );
            factory.Initialize(correctedRelativePotencyFactors.Keys, cumulativeConcentrationModels.Keys);
            var concentrations = factory.GenerateResidues(modelledFoods.First(), substances, random);
            Assert.IsNotNull(factory);
        }
    }
}
