using MCRA.General;
using MCRA.Simulation.Calculators.RiskPercentilesCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.SingleValueConcentrationsCalculation {

    [TestClass]
    public class RiskPercentilesCalculatorTests {

        /// <summary>
        /// Tests compute single value risks from risk single value concentrations
        /// calculator.
        /// </summary>
        [TestMethod]
        [DataRow(HealthEffectType.Risk, RiskMetricType.ExposureHazardRatio, new[] { 99D }, true)]
        [DataRow(HealthEffectType.Risk, RiskMetricType.HazardExposureRatio, new[] { 99D }, true)]
        [DataRow(HealthEffectType.Risk, RiskMetricType.ExposureHazardRatio, new[] { 99D }, false)]
        [DataRow(HealthEffectType.Risk, RiskMetricType.HazardExposureRatio, new[] { 99D }, false)]
        public void IndividualSingleValueRisksCalculator_TestCompute(
            HealthEffectType healthEffectType,
            RiskMetricType riskMetricType,
            double[] percentages,
            bool useInverseDistribution
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.CreateSimulated(100, 1, random);
            var individualEffects = FakeIndividualEffectsGenerator.Create(individuals, 0.1, random);
            var calculator = new RiskDistributionPercentilesCalculator(
                healthEffectType,
                riskMetricType,
                percentages,
                useInverseDistribution
            );
            var result = calculator.Compute(individualEffects);
            Assert.AreEqual(1, result.Count);
        }
    }
}
