using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Modelling;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class LogisticModelCalculatorCalculatorTests {

        [TestMethod]
        public void LogisticModelCalculator_TestComputeCovar() {
            var calculator = new LogisticModelCalculator();
            var ybin = new List<int>() { 0, 1, 2 };
            var nbin = new List<int>() { 2, 2, 2 };
            var weight = new List<double>() { 20, 60, 2000 };
            var designMatrix = new double[3, 2] { { 1, 1 }, { 1, 2 }, { 1, 3 } };
            var result = calculator.Compute(ybin, nbin, designMatrix, weight);

            Assert.HasCount(2, result.Estimates);
            Assert.IsFalse(double.IsNaN(result.LogLikelihood));
            Assert.IsGreaterThan(0.0, result.Dispersion);
            Assert.HasCount(designMatrix.GetLength(0), result.LinearPredictor);
        }

       /// <summary>
       /// Tests the compute method of the LogisticModelCalculator class using various
       /// bin counts and dispersion settings.
       /// </summary>
       /// <param name="nBin">The number of bins to use when generating test data for the logistic model.</param>
       /// <param name="fixedDispersion">true to use a fixed dispersion value in the computation; otherwise, 
       /// false to estimate dispersion.</param>
        [TestMethod]
        [DataRow(2, false)]
        [DataRow(2, true)]
        [DataRow(3, false)]
        [DataRow(3, true)]
        public void LogisticModelCalculator_TestCompute(
            int nBin,
            bool fixedDispersion
        ) {
            var mu = 2d;
            var variance = 1.5d;
            var n = 10000;
            var computeSes = true;
            (var yBin, var nBinomial, var designMatrix, var weights) = fakeInputData(mu, variance, nBin, n);

            var calculator = new LogisticModelCalculator();
            if (fixedDispersion) {
                var dispersion = variance;
                var result = calculator.Compute(yBin, nBinomial, designMatrix, weights, computeSes, dispersion);
                Assert.HasCount(1, result.Estimates);
                Assert.AreEqual(mu, result.Estimates[0], 0.1);
                Assert.AreEqual(variance, result.Dispersion);
                Assert.HasCount(designMatrix.GetLength(0), result.LinearPredictor);
                Assert.AreEqual(n - 1, result.DegreesOfFreedom);
            } else {
                var result = calculator.Compute(yBin, nBinomial, designMatrix, weights, computeSes);
                Assert.HasCount(1, result.Estimates);
                Assert.AreEqual(mu, result.Estimates[0], 0.1);
                Assert.AreEqual(variance, result.Dispersion, 0.1);
                Assert.HasCount(designMatrix.GetLength(0), result.LinearPredictor);
                Assert.AreEqual(n - 2, result.DegreesOfFreedom);
            }
        }

        [TestMethod]
        [DataRow(2, 0.01)]
        [DataRow(2, 10)]
        [DataRow(3, 0.01)]
        [DataRow(3, 10)]
        public void LogisticModelCalculator_TestComputeModelAssistedFrequency(
            int nBin,
            double variance
        ) {
            var mu = 2d;
            var n = 10000;
            var p = UtilityFunctions.ILogit(mu);
            (var yBin, var nBinomial, var designMatrix, var weights) = fakeInputData(mu, variance, nBin, n);

            var calculator = new LogisticModelCalculator();
            var result = calculator.Compute(yBin, nBinomial, designMatrix, weights, false, variance);
            var modelAssistedFrequencies = calculator
                .ComputeModelAssistedFrequency(
                    result.LinearPredictor,
                    result.Dispersion,
                    yBin,
                    nBinomial
                );
            Assert.HasCount(designMatrix.GetLength(0), modelAssistedFrequencies);
        }

        [TestMethod]
        [DataRow(2, 0.01)]
        [DataRow(2, 10)]
        [DataRow(3, 0.01)]
        [DataRow(3, 10)]
        public void LogisticModelCalculator_TestComputeFittedValues(
            int nBin,
            double variance
        ) {
            var mu = 2d;
            var n = 10000;
            (var yBin, var nBinomial, var designMatrix, var weights) = fakeInputData(mu, variance, nBin, n);
            var calculator = new LogisticModelCalculator();
            var result = calculator.Compute(yBin, nBinomial, designMatrix, weights, false, variance);
            var fittedValues = calculator.ComputeFittedValues(
                result.LinearPredictor,
                result.Dispersion,
                nBinomial,
                designMatrix.GetLength(0)
            );
            Assert.HasCount(designMatrix.GetLength(0), fittedValues);
        }

        private static (List<int> responses, List<int> nBinomial, double[,] designMatrix, List<double> weights) fakeInputData(
            double mu,
            double variance,
            int nBin,
            int n,
            int seed = 12345
        ) {
            var random = new McraRandomGenerator(seed);
            var normal = new NormalDistribution(mu, Math.Sqrt(variance));
            var rows = nBin + 1;

            var draws = new List<int>();
            for (int i = 0; i < n; i++) {
                var lp = normal.Draw(random);
                var prob = 1 / (1 + Math.Exp(-lp));
                var count = 0;
                for (int ii = 0; ii < nBin; ii++) {
                    count += random.NextDouble() > prob ? 0 : 1;
                }
                draws.Add(count);
            }

            var ybin = Enumerable.Range(0, rows).ToList();
            var nbin = Enumerable.Repeat(nBin, rows).ToList();
            var weights = new List<double>();
            var designMatrix = new double[rows, 1];
            for (int i = 0; i < rows; i++) {
                designMatrix[i, 0] = 1;
                weights.Add(draws.Count(c => c == i));
            }
            return (ybin, nbin, designMatrix, weights);
        }
    }
}
