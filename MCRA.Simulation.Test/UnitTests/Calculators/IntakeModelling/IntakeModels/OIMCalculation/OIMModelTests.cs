using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.OIMCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IntakeModelling.IntakeModels.OIMCalculation {

    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass]
    public class OIMModelTests {
        /// <summary>
        /// Calculate parameters OIM model
        /// </summary>
        [TestMethod]
        public void OIMModelTest1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var individualDayIntakes = FakeSimpleIndividualDayIntakeGenerator.Create(individualDays, 0.3, random);
            var oimModel = new OIMModel();
            oimModel.CalculateParameters(individualDayIntakes);
            //Assert.AreEqual(40, oimModel.DietaryIndividualDayIntakes.Count);
        }
    }
}
