using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.StatisticalTesting {

    /// <summary>
    /// Snedecor and Cochran, ch 10, Analysis of Variance
    /// </summary>
    [TestClass()]
    public class StatisticalTestingTests {


        #region Mock

        private List<double> _mu = new List<double>();
        private List<double> _sigma = new List<double>();
        private List<int> _numberOfObservations = new List<int>();

        /// <summary>
        /// Snedecor and Cochran p 259
        /// </summary>
        private void getMockData1() {
            var x1 = new List<double> { 64, 72, 68, 77, 56, 95 };
            var x2 = new List<double> { 78, 91, 97, 82, 85, 77 };
            var x3 = new List<double> { 75, 93, 78, 71, 63, 76 };
            var x4 = new List<double> { 55, 66, 49, 64, 70, 68 };
            _mu.Add(x1.Average());
            _mu.Add(x2.Average());
            _mu.Add(x3.Average());
            _mu.Add(x4.Average());
            _sigma.Add(Math.Sqrt(x1.Variance()));
            _sigma.Add(Math.Sqrt(x2.Variance()));
            _sigma.Add(Math.Sqrt(x3.Variance()));
            _sigma.Add(Math.Sqrt(x4.Variance()));
            _numberOfObservations.Add(x1.Count);
            _numberOfObservations.Add(x2.Count);
            _numberOfObservations.Add(x3.Count);
            _numberOfObservations.Add(x4.Count);
        }

        /// <summary>
        /// Snedecor and Cochran p 290
        /// </summary>
        private void getMockData2() {
            var x1 = new List<double> { 46, 31, 37, 62, 30 };
            var x2 = new List<double> { 70, 59 };
            var x3 = new List<double> { 52, 44, 57, 40, 67, 64, 70 };
            var x4 = new List<double> { 47, 21, 70, 46, 14 };
            var x5 = new List<double> { 42, 64, 50, 69, 77, 81, 87 };
            var x6 = new List<double> { 35, 68, 59, 38, 57, 76, 57, 29, 60 };
            _mu.Add(x1.Average());
            _mu.Add(x2.Average());
            _mu.Add(x3.Average());
            _mu.Add(x4.Average());
            _mu.Add(x5.Average());
            _mu.Add(x6.Average());
            _sigma.Add(Math.Sqrt(x1.Variance()));
            _sigma.Add(Math.Sqrt(x2.Variance()));
            _sigma.Add(Math.Sqrt(x3.Variance()));
            _sigma.Add(Math.Sqrt(x4.Variance()));
            _sigma.Add(Math.Sqrt(x5.Variance()));
            _sigma.Add(Math.Sqrt(x6.Variance()));
            _numberOfObservations.Add(x1.Count);
            _numberOfObservations.Add(x2.Count);
            _numberOfObservations.Add(x3.Count);
            _numberOfObservations.Add(x4.Count);
            _numberOfObservations.Add(x5.Count);
            _numberOfObservations.Add(x6.Count);
        }

        #endregion

        /// <summary>
        /// Equality of means test, p259 Snedecor
        /// </summary>
        [TestMethod()]
        public void StatisticalTesting_TestEqualityOfMeans1() {
            getMockData1();
            var fRatioStatistics = StatisticalTests.EqualityOfMeans(_mu, _sigma, _numberOfObservations);
        }

        /// <summary>
        /// Equality of means test, p290 Snedecor
        /// </summary>
        [TestMethod()]
        public void StatisticalTesting_TestEqualityOfMeans2() {
            getMockData2();
            var fRatioStatistics = StatisticalTests.EqualityOfMeans(_mu, _sigma, _numberOfObservations);
        }

        /// <summary>
        /// Homogeneity of variances,  p259 Snedecor
        /// </summary>
        [TestMethod()]
        public void StatisticalTesting_TestHomogeneityOfVariances1() {
            getMockData1();
            var bartlettsStatistics = StatisticalTests.HomogeneityOfVariances(_sigma, _numberOfObservations);
        }

        /// <summary>
        /// Homogeneity of variances,  p297 Snedecor
        /// </summary>
        [TestMethod()]
        public void StatisticalTesting_TestHomogeneityOfVariances2() {
            var numberOfObservations = new List<int> { 10, 8, 10, 8, 6 };
            var variances = new List<double> { 0.909, 0.497, 0.076, 0.103, 0.146 };
            var sigma = variances.Select(c => Math.Sqrt(c)).ToList();
            var bartlettsStatistics = StatisticalTests.HomogeneityOfVariances(sigma, numberOfObservations);
        }

        /// <summary>
        /// REF: A model for probabilistic health impact assessment of exposure to food chemicals, 
        /// 2008, Hilko van der Voet et al. Food and Chemical Toxicology.
        /// Nominal p50 = 0.36
        /// Uncertainty upper p95 = 0.50
        /// Nominal p50 p95 = 0.60
        /// Uncertainty upper p95 p95 = 0.75
        /// </summary>
        [TestMethod()]
        public void StatisticalTesting_TestGetDegreesOfFreedom() {
            var nominal = 0.36;
            var upper = 0.60;
            var nominalUncertainty = 0.50;
            var upperUncertainty = 0.75;
            var degreesOfFreedom = StatisticalTests.GetDegreesOfFreedom(nominal, upper, nominalUncertainty, upperUncertainty, true);
            Assert.AreEqual(7, degreesOfFreedom);
        }

        /// <summary>
        /// T-Test: p105 Snedecor
        /// </summary>
        [TestMethod()]
        public void StatisticalTesting_TestMeans() {
            var sample1 = new List<double> {134, 146, 104, 119, 124, 161, 107, 83, 113, 129, 97, 123};
            var sample2 = new List<double> {70, 118, 101, 85, 107, 132, 94 };
            var prob = StatisticalTests.TTest(sample1, sample2, true);
            Assert.AreEqual(0.076, prob, 1e-2);
        }
    }
}
