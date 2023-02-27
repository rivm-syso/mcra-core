using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.SingleValueConcentrationsCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var substances = MockSubstancesGenerator.Create(3);
            var foods = MockFoodsGenerator.Create(3);
            var sampleSubstanceCollection = MockSampleCompoundCollectionsGenerator.Create(foods, substances, random);
            var calculator = new SingleValueConcentrationsCalculator();
            var result = calculator.Compute(foods, substances, sampleSubstanceCollection.Values, null);
            Assert.AreEqual(9, result.Count);
            Assert.AreEqual(1, result.Count(r => double.IsNaN(r.Value.Loq)));
        }
    }
}
