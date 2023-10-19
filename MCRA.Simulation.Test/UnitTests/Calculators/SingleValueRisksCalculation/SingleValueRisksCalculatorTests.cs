using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;

namespace MCRA.Simulation.Test.UnitTests.Calculators.SingleValueConcentrationsCalculation {

    [TestClass]
    public class SingleValueRisksCalculatorTests {

        /// <summary>
        /// Tests compute highest residue single value concentrations using single value concentrations
        /// calculator.
        /// </summary>
        [TestMethod]
        public void SingleValueRisksCalculator_TestCompute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.CreateFoodsWithUnitWeights(5, random, fractionMissing: .1);
            var substances = MockSubstancesGenerator.Create(3);
            var exposures = MockSingleValueDietaryExposuresGenerator.Create(foods, substances, random);
            var exposuresUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed: seed);
            var hazardCharacterisationsUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var calculator = new SingleValueRisksCalculator();
            var result = calculator.Compute(
                exposures,
                hazardCharacterisations,
                exposuresUnit,
                hazardCharacterisationsUnit
            );

            Assert.AreEqual(exposures.Count, result.Count);
            Assert.IsTrue(result.All(r => Math.Abs(r.ExposureHazardRatio - 1 / r.HazardExposureRatio) < 1e-5));
        }
    }
}
