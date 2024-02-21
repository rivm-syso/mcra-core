using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {

    /// <summary>
    /// Tests the Dirichlet distribution function
    /// </summary>
    [TestClass()]
    public class DirichletDistributionTests : DistributionsTestsBase {

        private static double _epsilon = 1e-12;

        /// <summary>
        /// Tests the Dirichlet distribution function and 
        /// repeats 100000 times to validate the distribution pattern.
        /// </summary>
        [TestMethod()]
        public void DirichletDistribution_TestRepeatedSampling() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);

            const int repeat = 100000;
            var doTest = new Func<double[], bool>((prob) => {
                var n = prob.Length;
                var sums = new double[n];
                for (int i = 0; i < repeat; i++) {
                    seed = random.Next();
                    var shares = DirichletDistribution.Sample(prob, seed);
                    //test that the generated shares sum to 1
                    Assert.AreEqual(1, shares.Sum(), _epsilon);

                    //sum all individually created values per position in the array
                    for (int j = 0; j < n; j++) {
                        sums[j] += shares[j];
                    }
                }
                var sumProb = prob.Sum();
                //check whether the distributions are consistent with the probability array (prob)
                for (int i = 0; i < n; i++) {
                    //the sum of each value should roughly resemble the 
                    //part of it in the probability distribution
                    var part = prob[i] / sumProb;
                    var expected = repeat * part;
                    var delta = Math.Max(_epsilon, expected / 2);
                    Assert.AreEqual(expected, sums[i], delta);
                }
                return true;
            });

            doTest(new double[] { .1, .2, .3, .4 });
            doTest(new double[] { 10, 20, 30, 40 });
            doTest(new double[] { 1000, 2000, 3000, 4000 });
            doTest(new double[] { 1, 200, 3000, 40000 });
            doTest(new double[] { 1e-200, 2000, 3000, 1e20 });
            doTest(new double[] { 0, 0, 2000, 3000, 10, 5, 0.005 });
        }

        /// <summary>
        /// Tests the Dirichlet distribution function and 
        /// repeats 100000 times to validate the distribution pattern.
        /// Use extreme values in the input array
        /// </summary>
        [TestMethod()]
        public void DirichletDistribution_TestSampleMinMaxValues() {
            var seed = 1;
            var r = new McraRandomGenerator(seed);
            var prob = new double[] { 1E-300, 1E300 };
            var n = prob.Length;

            //test that the generated shares sum to 1
            const int repeat = 100000;

            var sums = new double[n];
            for (int i = 0; i < repeat; i++) {
                seed = r.Next();
                var shares = DirichletDistribution.Sample(prob, seed);
                Assert.AreEqual(1, shares.Sum(), _epsilon);

                //sum all individually created values per position in the array
                for (int j = 0; j < n; j++) {
                    sums[j] += shares[j];
                }
            }
            //check whether the distributions are consistent with the probability array (prob)
            Assert.AreEqual(0D, sums[0], _epsilon);
            Assert.AreEqual((double)repeat, sums[1], _epsilon);
        }

        /// <summary>
        /// Tests the Dirichlet distribution function with an input array containing zeroes.
        /// </summary>
        [TestMethod()]
        public void DirichletDistribution_TestSampleArrayContainingZeroTest() {
            var prob = new double[] { 1e-300, 0, 0, 0, 1e300 };
            var shares = DirichletDistribution.Sample(prob, 1);
            Assert.AreEqual(1, shares.Sum(), _epsilon);
        }

        /// <summary>
        /// Tests the Dirichlet distribution function with an empty input array.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void DirichletDistribution_TestSampleEmptyArray() {
            var prob = Array.Empty<double>();
            _ = DirichletDistribution.Sample(prob, 1);
        }

        /// <summary>
        /// Tests the Dirichlet distribution function with an input array of zeroes.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void DirichletDistribution_TestSampleArrayOfZeroes() {
            var prob = new double[] { 0, 0, 0, 0 };
            _ = DirichletDistribution.Sample(prob, 1);
        }

        /// <summary>
        /// Tests the Dirichlet distribution function with a null input array
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(NullReferenceException))]
        public void DirichletDistribution_TestSampleNullArray() {
            _ = DirichletDistribution.Sample(null, 1);
        }
    }
}
