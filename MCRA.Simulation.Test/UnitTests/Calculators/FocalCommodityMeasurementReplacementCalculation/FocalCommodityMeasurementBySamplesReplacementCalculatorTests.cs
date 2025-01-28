using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.FocalCommodityMeasurementReplacementCalculation {

    /// <summary>
    /// Tests for the focal commodity measurement replacement calculator factory.
    /// </summary>
    [TestClass]
    public class FocalCommodityMeasurementBySamplesReplacementCalculatorTests {

        /// <summary>
        /// Test focal commodity measurement by samples replacement calculator.
        /// </summary>
        [TestMethod]
        public void FocalCommodityMeasurementBySamplesReplacementCalculator_TestReplace() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(5);
            var focalFood = foods.First();
            var focalSubstance = substances.First();
            var focalCombinations = new List<(Food, Compound)>() { (focalFood, focalSubstance) };

            // Create background sample compound collection
            var backgroundSampleCompoundCollection = FakeSampleCompoundCollectionsGenerator
                .Create(foods, substances, random);

            // Create foreground sample compound collection with only one sample (for the focal food)
            // and one measurement (for the focal substance) with a specific concentration
            var foregroundSampleConcentration = 2;
            var focalSampleCompoundCollection = FakeSampleCompoundCollectionsGenerator
                .Create([focalFood], [focalSubstance], [(focalFood, [foregroundSampleConcentration])])
                .ToDictionary(r => r.Food);

            var adjustmentFactor = 0.5;
            var focalCommodityScenarioOccurrencePercentage = 50;

            // Create measurements replacement calculator and compute
            var model = new FocalCommodityMeasurementBySamplesReplacementCalculator(
                focalSampleCompoundCollection,
                null,
                focalCommodityScenarioOccurrencePercentage,
                adjustmentFactor,
                false,
                null
            );
            var result = model.Compute(
                backgroundSampleCompoundCollection,
                focalCombinations,
                random
            );

            // Check result available
            Assert.IsNotNull(result);

            // Check replaced concentrations
            var residuesFocalCombination = result[focalFood].SampleCompoundRecords
                .Select(r => r.SampleCompounds[focalSubstance].Residue)
                .Distinct()
                .ToArray();
            var expectedFocalCombinationResidues = new[] { 0, adjustmentFactor * foregroundSampleConcentration };
            CollectionAssert.AreEquivalent(expectedFocalCombinationResidues, residuesFocalCombination);
        }

        /// <summary>
        /// Test focal commodity measurement by samples replacement calculator.
        /// </summary>
        [TestMethod]
        public void FocalCommodityMeasurementBySamplesReplacementCalculator_TestReplaceProcessed() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var processingType = FakeProcessingTypesGenerator.CreateSingle("JUICING");
            var foods = FakeFoodsGenerator.Create(2);
            var focalFood = foods.First();
            var processedFocalFood = FakeFoodsGenerator.CreateProcessedFoods([focalFood], [processingType], "#").First();
            foods.Add(processedFocalFood);
            var substances = FakeSubstancesGenerator.Create(5);
            var focalSubstance = substances.First();
            var focalCombinations = new List<(Food, Compound)>() { (focalFood, focalSubstance) };

            // Create background sample compound collection
            var backgroundSampleCompoundCollection = FakeSampleCompoundCollectionsGenerator
                .Create(foods, substances, random);

            // Create foreground sample compound collection with only one sample (for the focal food)
            // and one measurement (for the focal substance) with a specific concentration
            var foregroundSampleConcentration = 2;
            var focalSampleCompoundCollection = FakeSampleCompoundCollectionsGenerator
                .Create([focalFood], [focalSubstance], [(focalFood, [foregroundSampleConcentration])])
                .ToDictionary(r => r.Food);

            var adjustmentFactor = 0.5;
            var focalCommodityScenarioOccurrencePercentage = 50;

            var processingFactor = new PFFixedModel(
                new ProcessingFactor() {
                    FoodUnprocessed = focalFood,
                    Compound = focalSubstance,
                    ProcessingType = processingType,
                    Nominal = 2
                }
            );
            processingFactor.CalculateParameters();
            var processingFactorProvider = new ProcessingFactorProvider([processingFactor], false, 1D);

            // Create measurements replacement calculator and compute
            var model = new FocalCommodityMeasurementBySamplesReplacementCalculator(
                focalSampleCompoundCollection,
                null,
                focalCommodityScenarioOccurrencePercentage,
                adjustmentFactor,
                true,
                processingFactorProvider
            );
            var result = model.Compute(
                backgroundSampleCompoundCollection,
                focalCombinations,
                random
            );

            // Check result available
            Assert.IsNotNull(result);

            // Check replaced concentrations for focal food
            var checkFoods = new List<Food>() { focalFood, processedFocalFood };
            foreach (var food in checkFoods) {
                var residuesFocalCombination = result[focalFood].SampleCompoundRecords
                    .Select(r => r.SampleCompounds[focalSubstance].Residue)
                    .Distinct()
                    .ToArray();
                var expectedFocalCombinationResidues = new[] { 0, adjustmentFactor * foregroundSampleConcentration };
                CollectionAssert.AreEquivalent(expectedFocalCombinationResidues, residuesFocalCombination);
            }
        }

        /// <summary>
        /// Test create measurement removal calculator.
        /// </summary>
        [TestMethod]
        public void FocalCommodityMeasurementBySamplesReplacementCalculator_TestSubstanceConversions() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(5);
            var backgroundSampleCompoundCollection = FakeSampleCompoundCollectionsGenerator
                .Create(foods, substances, random);
            var focalFood = foods.Take(1).First();
            var focalSubstance = substances.Take(1).First();
            var focalSampleCompoundCollection = FakeSampleCompoundCollectionsGenerator
                .Create([focalFood], [focalSubstance], random, numberOfSamples: [1]);

            var focalCombinationSubstanceConversionFactor = .4;
            var substanceConversions = new List<DeterministicSubstanceConversionFactor>() {
                new() {
                    MeasuredSubstance = focalSubstance,
                    ActiveSubstance = focalSubstance,
                    ConversionFactor = focalCombinationSubstanceConversionFactor
                }
            };

            var adjustmentFactor = 0.5;
            var model = new FocalCommodityMeasurementBySamplesReplacementCalculator(
                focalSampleCompoundCollection,
                substanceConversions,
                50,
                adjustmentFactor,
                false,
                null
            );
            var focalCombinations = foods.Take(1).SelectMany(r => substances.Take(1), (f, s) => (Food: f, Substance: s)).ToList();
            var result = model.Compute(
                backgroundSampleCompoundCollection,
                focalCombinations,
                random
            );

            // Check result available
            Assert.IsNotNull(result);

            // Check replaced concentrations
            var focalSampleCompoundRecord = focalSampleCompoundCollection[focalFood].SampleCompoundRecords.First().SampleCompounds[focalSubstance];
            var expectedPositiveValue = focalCombinationSubstanceConversionFactor *
                (focalSampleCompoundRecord.IsCensoredValue ? focalSampleCompoundRecord.Lor : adjustmentFactor * focalSampleCompoundRecord.Residue);
            var residuesFocalCombination = result[focalFood].SampleCompoundRecords
                .Select(r => r.SampleCompounds[focalSubstance])
                .Select(r => r.IsCensoredValue ? r.Lor : r.Residue)
                .Distinct().ToArray();
            CollectionAssert.AreEquivalent(new double[] { 0, expectedPositiveValue }, residuesFocalCombination);
        }
    }
}
