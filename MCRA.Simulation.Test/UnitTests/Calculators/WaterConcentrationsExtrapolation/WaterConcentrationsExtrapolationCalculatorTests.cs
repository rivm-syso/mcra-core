using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.FoodExtrapolationsCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.General.ModuleDefinitions.Interfaces;

namespace MCRA.Simulation.Test.UnitTests.Calculators.WaterConcentrationsExtrapolation {

    /// <summary>
    /// Water concentrations extrapolation calculator tests.
    /// </summary>
    [TestClass]
    public class WaterConcentrationsExtrapolationCalculatorTests {

        internal class MockWaterConcentrationsExtrapolationCalculatorSettings : IWaterConcentrationsExtrapolationCalculatorSettings {
            public double WaterConcentrationValue { get; set; }
            public bool RestrictWaterImputationToAuthorisedUses { get; set; }
            public bool RestrictWaterImputationToApprovedSubstances { get; set; }
            public bool RestrictWaterImputationToMostPotentSubstances { get; set; }
        }

        /// <summary>
        /// Calculates water concentrations extrapolation, needs further implementation by Johannes
        /// </summary>
        [TestMethod]
        public void WaterConcentrationsExtrapolationCalculator_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(5);
            var water = new Food() {
                Code = "Water",
                Name = "Water"
            };
            var substances = FakeSubstancesGenerator.Create(4);
            var rpfs = substances
                .Select((r, ix) => new {
                    Substance = r,
                    Rpf = (double)ix + 1
                })
                .ToDictionary(c => c.Substance, c => c.Rpf);
            var sampleCompoundsCollections = FakeSampleCompoundCollectionsGenerator
                .Create(foods, substances, random);

            var settings = new MockWaterConcentrationsExtrapolationCalculatorSettings() {
                WaterConcentrationValue = 0.2,
                RestrictWaterImputationToAuthorisedUses = false,
                RestrictWaterImputationToApprovedSubstances = true,
                RestrictWaterImputationToMostPotentSubstances = true
            };
            var calculator = new WaterConcentrationsExtrapolationCalculator(settings);
            var result = calculator.Create(
                substances,
                water,
                null,
                null,
                2,
                rpfs,
                ConcentrationUnit.mgPerKg
             );

            Assert.IsNotNull(result);
            Assert.AreEqual(water, result.Food);
            Assert.HasCount(1, result.SampleCompoundRecords);
            var sampleCompoundRecord = result.SampleCompoundRecords.First().SampleCompounds;

            Assert.IsTrue(sampleCompoundRecord[substances[0]].IsZeroConcentration);
            Assert.IsTrue(sampleCompoundRecord[substances[1]].IsZeroConcentration);
            Assert.IsTrue(sampleCompoundRecord[substances[2]].IsPositiveResidue);
            Assert.IsTrue(sampleCompoundRecord[substances[3]].IsPositiveResidue);
        }

        [TestMethod]
        public void ImputeWater_RestrictBySubstanceApprovals_ShouldHavePositiveResiduesForApprovedSubstancesOnly() {
            var water = new Food() {
                Code = "Water",
                Name = "Water"
            };
            var substances = FakeSubstancesGenerator.Create(7);
            var substanceApprovals = FakeSubstanceApprovalsGenerator.Create(substances).ToDictionary(c => c.Substance);
            var rpfs = substances
                .Select((r, ix) => new {
                    Substance = r,
                    Rpf = (double)ix + 1
                })
                .ToDictionary(c => c.Substance, c => c.Rpf);

            var settings = new MockWaterConcentrationsExtrapolationCalculatorSettings() {
                WaterConcentrationValue = 0.2,
                RestrictWaterImputationToAuthorisedUses = false,
                RestrictWaterImputationToApprovedSubstances = true,
                RestrictWaterImputationToMostPotentSubstances = true
            };

            var calculator = new WaterConcentrationsExtrapolationCalculator(settings);
            var result = calculator.Create(
                substances,
                water,
                authorisations: null,
                substanceApprovals,
                numberOfSubstances: 5,
                rpfs,
                ConcentrationUnit.mgPerKg
             );

            Assert.IsNotNull(result);
            Assert.AreEqual(water, result.Food);
            Assert.HasCount(1, result.SampleCompoundRecords);
            var sampleCompoundRecord = result.SampleCompoundRecords.First().SampleCompounds;
            foreach (var record in sampleCompoundRecord) {
                Assert.IsTrue(substanceApprovals[record.Key].IsApproved ? sampleCompoundRecord[record.Key].IsPositiveResidue
                                                                        : sampleCompoundRecord[record.Key].IsZeroConcentration);
            }
        }

        [TestMethod]
        public void ImputeWater_NoRestrictionsBySubstanceApprovals_ShouldHavePositiveResiduesForMostPotentSubstances() {
            var water = new Food() {
                Code = "Water",
                Name = "Water"
            };
            var substances = FakeSubstancesGenerator.Create(7);
            var rpfs = substances
                .Select((r, ix) => new {
                    Substance = r,
                    Rpf = (double)ix + 1
                })
                .ToDictionary(c => c.Substance, c => c.Rpf);

            var settings = new MockWaterConcentrationsExtrapolationCalculatorSettings() {
                WaterConcentrationValue = 0.2,
                RestrictWaterImputationToAuthorisedUses = false,
                RestrictWaterImputationToApprovedSubstances = false,
                RestrictWaterImputationToMostPotentSubstances = true
            };

            var nrOfmostToxicSubstances = 5;
            var calculator = new WaterConcentrationsExtrapolationCalculator(settings);
            var result = calculator.Create(
                substances,
                water,
                authorisations: null,
                substanceApprovals: null,
                nrOfmostToxicSubstances,
                rpfs,
                ConcentrationUnit.mgPerKg
             );

            Assert.IsNotNull(result);
            Assert.AreEqual(water, result.Food);
            Assert.HasCount(1, result.SampleCompoundRecords);
            var sampleCompoundRecord = result.SampleCompoundRecords.First().SampleCompounds;

            var rpfLowerLimit = rpfs.Select(kv => kv.Value).ToList()[rpfs.Count - nrOfmostToxicSubstances];
            foreach (var record in sampleCompoundRecord) {
                var substance = record.Key;
                var rpf = rpfs[substance];
                Assert.IsTrue(rpf >= rpfLowerLimit ? sampleCompoundRecord[substance].IsPositiveResidue : sampleCompoundRecord[substance].IsZeroConcentration);
            }
        }
    }
}
