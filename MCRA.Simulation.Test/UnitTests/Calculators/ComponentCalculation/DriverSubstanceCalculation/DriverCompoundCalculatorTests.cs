using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.MixtureCalculation {
    /// <summary>
    /// MixtureCalculation calculator
    /// </summary>
    [TestClass]
    public class DriverCompoundCalculatorTests {
        /// <summary>
        /// Calculate driver substances
        /// </summary>
        [TestMethod]
        public void DriverCompoundCalculator_TestCompute1() {
            for (int i = 0; i < 100; i++) {
                var random = new McraRandomGenerator(i);
                var numIndividuals = i < 10 ? 10 : i;
                var zeroExposureIndividuals = random.Next(0, numIndividuals / 2);
                var numSubstances = 2 + i / 5;
                var zeroExposureSubstances = numSubstances > 10 ? random.Next(0, numSubstances / 5) : 0;
                var numComponents = random.Next(1, numSubstances - zeroExposureSubstances);

                var individuals = MockIndividualsGenerator.Create(numIndividuals, 1, random);
                var individualIds = individuals.Select(r => r.Id).ToList();
                var substances = MockSubstancesGenerator.Create(numSubstances);
                var matrix = MockComponentGenerator.CreateExposureMatrix(individualIds, substances, numComponents, zeroExposureSubstances, zeroExposureIndividuals);

                var percentage = 100 - (numIndividuals - zeroExposureIndividuals) / (double)numIndividuals * 100;
             
                var driverSubstances = DriverSubstanceCalculator.CalculateExposureDrivers(matrix);

                Assert.AreEqual(individuals.Count - zeroExposureIndividuals, driverSubstances.Where(c => c.CumulativeExposure > 0).Count());
                Assert.AreEqual(driverSubstances.Count, numIndividuals);
            }
        }
        /// <summary>
        /// Calculate components
        /// </summary>
        [TestMethod]
        public void DriverCompoundCalculator_TestCompute2() {
            for (int i = 0; i < 100; i++) {
                var random = new McraRandomGenerator(i);
                var numIndividuals = i < 10 ? 10 : i;
                var zeroExposureIndividuals = random.Next(0, numIndividuals / 2);
                var numSubstances = 2 + i / 5;
                var zeroExposureSubstances = numSubstances > 10 ? random.Next(0, numSubstances / 5) : 0;
                var numComponents = random.Next(1, numSubstances - zeroExposureSubstances);

                var individuals = MockIndividualsGenerator.Create(numIndividuals, 1, random);
                var individualIds = individuals.Select(r => r.Id).ToList();
                var substances = MockSubstancesGenerator.Create(numSubstances);
                var matrix = MockComponentGenerator.CreateComponents(substances, random, numComponents, zeroExposureSubstances, zeroExposureIndividuals);
                Assert.IsNotNull(matrix);
            }
        }
    }
}
