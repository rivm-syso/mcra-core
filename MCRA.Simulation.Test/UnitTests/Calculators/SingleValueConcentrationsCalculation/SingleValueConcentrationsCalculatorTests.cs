using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.SingleValueConcentrationsCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.SingleValueConcentrationsCalculation {

    [TestClass]
    public class SingleValueConcentrationsCalculatorTests {

        /// <summary>
        /// Tests compute highest residue single value concentrations using single value concentrations
        /// calculator.
        /// </summary>
        [TestMethod]
        public void SingleValueConcentrationsCalculator_TestCompute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(3);
            var foods = FakeFoodsGenerator.Create(3);
            var sampleSubstanceCollection = FakeSampleCompoundCollectionsGenerator.Create(foods, substances, random);
            var calculator = new SingleValueConcentrationsCalculator();
            var result = calculator.Compute(foods, substances, sampleSubstanceCollection.Values, null);
            Assert.HasCount(9, result);
            Assert.AreEqual(1, result.Count(r => double.IsNaN(r.Value.Loq)));
        }
    }
}
