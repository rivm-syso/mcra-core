using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.FocalCommodityMeasurementReplacementCalculation {

    /// <summary>
    /// Tests for the focal commodity measurement replacement calculator factory.
    /// </summary>
    [TestClass]
    public class FocalCommodityMeasurementBySamplesReplacementCalculatorTests {

        /// <summary>
        /// Test create measurement removal calculator.
        /// </summary>
        [TestMethod]
        public void FocalCommodityMeasurementBySamplesReplacementCalculator_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(5);
            var backgroundSampleCompoundCollection = MockSampleCompoundCollectionsGenerator
                .Create(foods, substances, random);
            var focalSampleCompoundCollection = MockSampleCompoundCollectionsGenerator
                .Create(foods.Take(1).ToList(), substances.Take(1).ToList(), random, numberOfSamples: new int[] { 1 });

            var adjustmentFactor = 0.5;
            var model = new FocalCommodityMeasurementBySamplesReplacementCalculator(
                focalSampleCompoundCollection,
                null,
                50,
                adjustmentFactor
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
            var residuesFocalCombination = result[foods.First()].SampleCompoundRecords.Select(r => r.SampleCompounds[substances.First()].Residue).Distinct().ToArray();
            var expectedFocalCombinationResidues = new double[] { 0, adjustmentFactor * focalSampleCompoundCollection[foods.First()].SampleCompoundRecords.First().SampleCompounds.First().Value.Residue };
            CollectionAssert.AreEquivalent(expectedFocalCombinationResidues, residuesFocalCombination);
        }

        /// <summary>
        /// Test create measurement removal calculator.
        /// </summary>
        [TestMethod]
        public void FocalCommodityMeasurementBySamplesReplacementCalculator_TestSubstanceConversions() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(5);
            var backgroundSampleCompoundCollection = MockSampleCompoundCollectionsGenerator
                .Create(foods, substances, random);
            var focalFood = foods.Take(1).First();
            var focalSubstance = substances.Take(1).First();
            var focalSampleCompoundCollection = MockSampleCompoundCollectionsGenerator
                .Create([focalFood], [focalSubstance], random, numberOfSamples: new int[] { 1 });

            var focalCombinationSubstanceConversionFactor = .4;
            var substanceConversions = new List<DeterministicSubstanceConversionFactor>() {
                new DeterministicSubstanceConversionFactor() {
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
                adjustmentFactor
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
