using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var exposuresUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay);
            var hazardCharacterisations = MockHazardCharacterisationModelsGenerator.Create(new Effect(), substances, seed: seed);
            var hazardCharacterisationsUnit = new TargetUnit(ExposureUnit.ugPerKgBWPerDay);

            var calculator = new SingleValueRisksCalculator();

            var result = calculator.Compute(
                exposures,
                hazardCharacterisations,
                exposuresUnit,
                hazardCharacterisationsUnit);

            Assert.AreEqual(exposures.Count, result.Count);
            Assert.IsTrue(result.All(r => Math.Abs(r.HazardQuotient - 1 / r.MarginOfExposure) < 1e-5));
        }

        /// <summary>
        /// Tests compute single value risks from risk single value concentrations
        /// calculator.
        /// </summary>
        [TestMethod]
        public void IndividualSingleValueRisksCalculator_TestCompute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(100, 1, random);
            var individualEffects = MockIndividualEffectsGenerator.Create(individuals, 0.1, random);
            var settings = new IndividualSingleValueRisksCalculatorSettings(new EffectModelSettingsDto() {
                HealthEffectType = HealthEffectType.Risk,
                Percentage = 0.1,
                RiskMetricType = RiskMetricType.MarginOfExposure,
                IsInverseDistribution = false
            });
            var calculator = new IndividualSingleValueRisksCalculator(settings);
            var result = calculator.Compute(individualEffects);
            Assert.AreEqual(1, result.Count);
            //Assert.IsTrue(result.All(r => Math.Abs(r.HazardQuotient - 1 / r.MarginOfExposure) < 1e-5));
        }
    }
}
