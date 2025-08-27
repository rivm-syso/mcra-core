using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.IntakeModelling;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling.IndividualFrequencyCalculation {

    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass]
    public class IndividualFrequencyCalculatorTests {

        /// <summary>
        /// Test compute individual frequencies.
        /// </summary>
        [TestMethod]
        public void IndividualFrequencyCalculator_TestAll() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var exposures = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.3, random);
            var frequencies = IndividualFrequencyCalculator.Compute(exposures);
            Assert.AreEqual(individuals.Count, frequencies.Count);
        }
    }
}