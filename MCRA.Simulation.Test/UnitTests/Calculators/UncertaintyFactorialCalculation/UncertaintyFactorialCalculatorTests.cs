using MCRA.General;
using MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.UncertaintyFactorialCalculation {

    /// <summary>
    /// UncertaintyFactorialCalculator tests.
    /// </summary>
    [TestClass]
    public class UncertaintyFactorialCalculatorTests {

        /// <summary>
        /// Test uncertainty results with uncertainty sources.
        /// </summary>
        [TestMethod]
        public void UncertaintyFactorialCalculator_TestCompute() {
            var design = new double[,] { { 1, 0, 0 }, { 1, 1, 0 }, { 1, 0, 1 }, { 1, 1, 1 } };
            var source = new List<string> { "mc", "conc", "ind" };
            var u1 = UncertaintySource.Concentrations;
            var u2 = UncertaintySource.Individuals;
            var taggedValues = new List<(List<UncertaintySource>, List<double>)> {
                ([], [1, 2]),
                ([], [2, 4]),
                ([u1], [1, 2]),
                ([u1], [3, 4]),
                ([u2], [1, 2]),
                ([u2], [4, 4]),
                ([u1, u2], [1, 2]),
                ([u1, u2], [5, 4])
            };

            var ufc = new PercentilesUncertaintyFactorialCalculator();
            var results = ufc.Compute(
                taggedValues,
                [90, 95],
                source,
                design);

            Assert.AreEqual(2, results.Count);
        }
    }
}
