using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ActiveSubstanceAllocation;
using MCRA.Simulation.Calculators.FoodConcentrationModelBuilders;
using MCRA.Simulation.Calculators.SampleCompoundCollections;
using MCRA.Simulation.Calculators.SampleCompoundCollections.MissingValueImputation;
using MCRA.Simulation.Calculators.SampleCompoundCollections.NonDetectsImputation;
using MCRA.Simulation.Test.Helpers;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Simulation.Test.Mock.MockCalculatorSettings;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ConcentrationModelCalculation {
    /// <summary>
    /// ResidueGeneration calculator
    /// </summary>
    [TestClass]
    public class FoodConcentrationModelsBuilderTests {

        /// <summary>
        /// Creates concentration models
        /// </summary>
        [TestMethod]
        public void FoodConcentrationModelsBuilder_TestCreate() {
            var outputPath = TestUtilities.CreateTestOutputPath("ConcentrationModelCalculationTests");
            var dataFolder = Path.Combine("Resources", "ConcentrationModelling");
            var targetFileName = Path.Combine(outputPath, "ConcentrationModelling.zip");
            var dataManager = TestUtilities.CompiledDataManagerFromFolder(dataFolder, targetFileName);
            var allSubstances = dataManager.GetAllCompounds().Values;
            var foods = dataManager.GetAllFoods().Values;
            var foodSamples = dataManager.GetAllFoodSamples().Values;

            var residueDefinitions = dataManager.GetAllSubstanceConversions();
            var concentrationUnit = ConcentrationUnit.mgPerKg;

            var measuredSubstances = residueDefinitions.Select(r => r.MeasuredSubstance).Distinct().ToList();
            var activeSubstances = residueDefinitions.Select(r => r.ActiveSubstance).ToHashSet();
            var sampleCompoundCollections = SampleCompoundCollectionsBuilder.Create(foods, measuredSubstances, foodSamples, concentrationUnit, null);

            var csvWriter = new SampleCompoundCollectionCsvWriter() {
                PrintLocation = false
            };
            csvWriter.WriteCsv(sampleCompoundCollections.Values, measuredSubstances, Path.Combine(outputPath, "SampleConcentrations-Raw.csv"), false, false);
            var compoundResidueCollections = FakeFoodSubstanceResidueCollectionsGenerator.Create(allSubstances, sampleCompoundCollections);

            var settings = new MockConcentrationModelCalculationSettings() {
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByZero,
                DefaultConcentrationModel = ConcentrationModelType.Empirical,
                ConcentrationModelTypesPerFoodCompound = [],
                CorrelateImputedValueWithSamplePotency = true,
                RestrictLorImputationToAuthorisedUses = false
            };

            var concentrationModelsBuilder = new FoodConcentrationModelsBuilder(settings);
            var concentrationModels = concentrationModelsBuilder.Create(
                foods,
                measuredSubstances,
                compoundResidueCollections,
                null,
                null,
                null,
                null,
                ConcentrationUnit.mgPerKg
            );

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var randomSubstanceAllocator = new RandomActiveSubstanceAllocationCalculator(residueDefinitions, null, true);
            var activeSubstanceSampleCompoundCollections = randomSubstanceAllocator.Allocate(sampleCompoundCollections.Values, activeSubstances, random);
            csvWriter.WriteCsv(activeSubstanceSampleCompoundCollections, activeSubstances, Path.Combine(outputPath, "SampleConcentrations-ActiveSubstance.csv"), false, false);

            var nonDetectsImputationCalculator = new CensoredValuesImputationCalculator(settings);
            nonDetectsImputationCalculator.ReplaceCensoredValues(sampleCompoundCollections.Values, concentrationModels, 0);
            csvWriter.WriteCsv(sampleCompoundCollections.Values, measuredSubstances, Path.Combine(outputPath, "SampleConcentrations-ND-Replaced.csv"), true, false);

            var correctedRpfs = measuredSubstances.ToDictionary(r => r, r => 1D);
            var missingValuesImputationCalculator = new MissingvalueImputationCalculator(settings);
            missingValuesImputationCalculator.ImputeMissingValues(sampleCompoundCollections.Values, concentrationModels, correctedRpfs, 0);
            csvWriter.WriteCsv(sampleCompoundCollections.Values, measuredSubstances, Path.Combine(outputPath, "SampleConcentrations-MV-Replaced.csv"), true, true);
        }

        /// <summary>
        /// Test option to restrict censored value imputation with positive values
        /// (e.g., based on LOR) to authorised uses only. When this option is checked,
        /// then for unauthorised uses, the use-fraction should be equal to the
        /// observed fraction positives.
        /// </summary>
        [DataRow(true, true)]
        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataRow(false, false)]
        [TestMethod]
        public void FoodConcentrationModelsBuilder_TestRestrictLorImputationToAuthorisedUses(
            bool authorised,
            bool restrictLorInmputationToAuthorisedUses
        ) {
            var foods = FakeFoodsGenerator.Create(1);
            var substances = FakeSubstancesGenerator.Create(1);
            var compoundResidueCollections = FakeFoodSubstanceResidueCollectionsGenerator
                .Create(
                    foods,
                    substances,
                    mean: 0.1,
                    upper: 0.2,
                    lods: [0.05],
                    loqs: [0.1],
                    fractionZero: .2,
                    sampleSize: 100
                );

            var settings = new MockConcentrationModelCalculationSettings() {
                NonDetectsHandlingMethod = NonDetectsHandlingMethod.ReplaceByLOR,
                DefaultConcentrationModel = ConcentrationModelType.Empirical,
                ConcentrationModelTypesPerFoodCompound = [],
                CorrelateImputedValueWithSamplePotency = true,
                RestrictLorImputationToAuthorisedUses = restrictLorInmputationToAuthorisedUses
            };

            var foodSubstances = foods.SelectMany(r => substances, (f, s) => (f, s)).ToList();
            var authorisations = authorised
                ? foodSubstances
                    .ToDictionary(r => r, r => new SubstanceAuthorisation() {
                        Food = r.f,
                        Substance = r.s
                    })
                : new Dictionary<(Food, Compound), SubstanceAuthorisation>();

            var concentrationModelsBuilder = new FoodConcentrationModelsBuilder(settings);
            var concentrationModels = concentrationModelsBuilder
                .Create(
                    foodSubstances,
                    compoundResidueCollections,
                    null,
                    null,
                    null,
                    authorisations,
                    ConcentrationUnit.mgPerKg
                );

            var observedFractionPositives = compoundResidueCollections.First().Value.FractionPositives;
            var computedUseFraction = concentrationModels.First().Value.OccurenceFraction;
            var expectedUseFraction = authorised || !restrictLorInmputationToAuthorisedUses ? 1 : observedFractionPositives;
            Assert.AreEqual(expectedUseFraction, computedUseFraction);
        }
    }
}
