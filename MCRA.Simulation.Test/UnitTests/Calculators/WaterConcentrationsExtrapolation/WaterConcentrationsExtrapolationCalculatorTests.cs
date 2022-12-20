using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.FoodExtrapolationsCalculation;
using MCRA.Simulation.Calculators.WaterConcentrationsExtrapolation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.Calculators.WaterConcentrationsExtrapolation {

    /// <summary>
    /// Water concentrations extrapolation calculator tests.
    /// </summary>
    [TestClass]
    public class WaterConcentrationsExtrapolationCalculatorTests {

        internal class MockWaterConcentrationsExtrapolationCalculatorSettings : IWaterConcentrationsExtrapolationCalculatorSettings {
            public double WaterConcentrationValue { get; set; }
            public bool RestrictWaterImputationToAuthorisedUses { get; set; }
            public bool RestrictWaterImputationToMostPotentSubstances { get; set; }
        }

        /// <summary>
        /// Calculates water concentrations extrapolation, needs further implementation by Johannes
        /// </summary>
        [TestMethod]
        public void WaterConcentrationsExtrapolationCalculator_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(5);
            var water = new Food() {
                Code = "Water",
                Name = "Water"
            };
            var substances = MockSubstancesGenerator.Create(4);
            var rpfs = substances
                .Select((r,ix) => new {
                    Substance = r,
                    Rpf = (double)ix + 1
                })
                .ToDictionary(c => c.Substance, c => c.Rpf);
            var sampleCompoundsCollections = MockSampleCompoundCollectionsGenerator
                .Create(foods, substances, random)
                .ToDictionary(c => c.Food, c => c);

            var settings = new MockWaterConcentrationsExtrapolationCalculatorSettings() {
                WaterConcentrationValue = 0.2,
                RestrictWaterImputationToAuthorisedUses = false,
                RestrictWaterImputationToMostPotentSubstances = true
            };
            var calculator = new WaterConcentrationsExtrapolationCalculator(settings);
            var result = calculator.Create(
                substances,
                water,
                null,
                2,
                rpfs,
                ConcentrationUnit.mgPerKg
             );

            Assert.AreEqual(water, result.Food);
            Assert.AreEqual(1, result.SampleCompoundRecords.Count);
            var sampleCompoundRecord = result.SampleCompoundRecords.First().SampleCompounds;

            Assert.IsTrue(sampleCompoundRecord[substances[0]].IsZeroConcentration);
            Assert.IsTrue(sampleCompoundRecord[substances[1]].IsZeroConcentration);
            Assert.IsTrue(sampleCompoundRecord[substances[2]].IsPositiveResidue);
            Assert.IsTrue(sampleCompoundRecord[substances[3]].IsPositiveResidue);
            Assert.IsNotNull(result);
        }
    }
}
