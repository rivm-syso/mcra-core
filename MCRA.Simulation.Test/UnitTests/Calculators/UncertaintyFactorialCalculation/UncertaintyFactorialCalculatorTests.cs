using MCRA.General;
using MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
                (new List<UncertaintySource> { }, new List<double> { 1, 2 }),
                (new List<UncertaintySource> { }, new List<double> { 2, 4 }),
                (new List<UncertaintySource> { u1 }, new List<double> { 1, 2 }),
                (new List<UncertaintySource> { u1 }, new List<double> { 3, 4 }),
                (new List<UncertaintySource> { u2 }, new List<double> { 1, 2 }),
                (new List<UncertaintySource> { u2 }, new List<double> { 4, 4 }),
                (new List<UncertaintySource> { u1, u2 }, new List<double> { 1, 2 }),
                (new List<UncertaintySource> { u1, u2 }, new List<double> { 5, 4 })
            };

            var ufc = new PercentilesUncertaintyFactorialCalculator();
            var results = ufc.Compute(
                taggedValues,
                new double[] { 90, 95 },
                source,
                design);

            Assert.AreEqual(2, results.Count);
        }
    }
}
