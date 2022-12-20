using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.Calculators.FocalCommodityMeasurementReplacementCalculation {

    /// <summary>
    /// Tests for the focal commodity measurement replacement calculator factory.
    /// </summary>
    [TestClass]
    public class FocalCommodityMeasurementRemovalCalculatorTests {

        /// <summary>
        /// Test create measurement removal calculator.
        /// </summary>
        [TestMethod]
        public void FocalCommodityMeasurementRemovalCalculator_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(5);
            var backgroundSampleCompoundCollection = MockSampleCompoundCollectionsGenerator
                .Create(foods, substances, random)
                .ToDictionary(r => r.Food);
            var mrls = MockMaximumConcentrationLimitsGenerator.Create(foods, substances, random);
            var model = new FocalCommodityMeasurementRemovalCalculator();
            var focalCombinations = foods.Take(1).SelectMany(r => substances.Take(1), (f, s) => (Food: f, Substance: s)).ToList();
            var result = model.Compute(
                backgroundSampleCompoundCollection,
                focalCombinations,
                null
            );

            // Check result available
            Assert.IsNotNull(result);

            Assert.IsTrue(result[foods.First()].SampleCompoundRecords.All(r => r.SampleCompounds[substances.First()].IsMissingValue));
        }
    }
}
