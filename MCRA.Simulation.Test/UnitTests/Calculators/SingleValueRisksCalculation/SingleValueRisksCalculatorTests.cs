using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

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
            var foods = FakeFoodsGenerator.CreateFoodsWithUnitWeights(5, random, fractionMissing: .1);
            var substances = FakeSubstancesGenerator.Create(3);
            var exposures = FakeSingleValueDietaryExposuresGenerator.Create(foods, substances, random);
            var exposuresUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisations = FakeHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed: seed);
            var hazardCharacterisationsUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);
            var calculator = new SingleValueRisksCalculator();
            var result = calculator.Compute(
                exposures,
                hazardCharacterisations,
                exposuresUnit,
                hazardCharacterisationsUnit
            );

            Assert.HasCount(exposures.Count, result);
            Assert.IsTrue(result.All(r => Math.Abs(r.ExposureHazardRatio - 1 / r.HazardExposureRatio) < 1e-5));
        }
    }
}
