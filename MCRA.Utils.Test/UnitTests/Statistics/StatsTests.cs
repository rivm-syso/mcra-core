using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class StatsTests {

        private class DoubleAsFloatEqualityComparer : EqualityComparer<double> {
            public override bool Equals(double x, double y) {
                return ((float)x).Equals((float)y);
            }
            public override int GetHashCode(double obj) {
                return ((float)obj).GetHashCode();
            }
        }

        [TestMethod]
        public void WeightedMean_Test1() {
            var x = new List<double>() { 10, 20, 30, 50, 80 };
            var w = new List<double>() { 4, 1, 1, 2, 1 };
            var mean = x.Average(w);
            Assert.AreEqual(30, mean);
        }

        [TestMethod]
        [TestCategory("Sandbox Tests")]
        public void WeightedMean_PerformanceTest() {
            var seed = 1;
            var n = 10_000_000;
            var ranGenerator = new McraRandomGenerator(seed);
            var x = new List<double>(n);
            var w = new List<double>(n);
            for (int i = 0; i < n; i++) {
                x.Add(ranGenerator.NextDouble(0, 1));
                w.Add(ranGenerator.NextDouble(0, 1));
            }
            var sw = new Stopwatch();
            sw.Start();
            var var = x.Average(w);
            sw.Stop();
            Assert.IsTrue(sw.ElapsedMilliseconds < 3000);
        }

        [TestMethod]
        public void WeightedVariance_Test1() {
            var x = new List<double>() { 10, 20, 30, 50, 80 };
            var w = new List<double>() { 4, 1, 1, 2, 1 };
            var var = x.Variance(w);
            Assert.AreEqual(625, var);
        }

        

        [TestMethod]
        [TestCategory("Sandbox Tests")]
        public void Percentiles_Benchmark() {
            var rg = new McraRandomGenerator();
            var numbers = new List<double>();
            for (int i = 0; i < 1_000_000; i++) {
                numbers.Add(rg.NextDouble());
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var p50 = numbers.AsEnumerable().Percentile(50);
            sw.Stop();
            Trace.WriteLine($"Elapsed: {sw.Elapsed}");
            Trace.WriteLine(p50);
            sw.Restart();
            var p50new = numbers.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToArray().PercentileSorted(50);
            sw.Stop();
            Trace.WriteLine($"Elapsed: {sw.Elapsed}");
            Trace.WriteLine(p50new);
        }

        [TestMethod]
        [TestCategory("Sandbox Tests")]
        public void SortingBenchmarkTestSortWithoutNaNs() {
            var rg = new McraRandomGenerator();
            var numbers = new List<double>();
            for (int i = 0; i < 1_000_000; i++) {
                numbers.Add(rg.NextDouble());
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var s1 = numbers.AsEnumerable().SortWithoutNaNs();
            sw.Stop();
            Trace.WriteLine($"Elapsed: {sw.Elapsed}");
        }

        [TestMethod]
        [TestCategory("Sandbox Tests")]
        public void SortingBenchmarkTestOrderBy() {
            var rg = new McraRandomGenerator();
            var numbers = new List<double>();
            for (int i = 0; i < 1_000_000; i++) {
                numbers.Add(rg.NextDouble());
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var s2 = numbers.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToArray();
            sw.Stop();
            Trace.WriteLine($"Elapsed: {sw.Elapsed}");
        }

        [TestMethod]
        [TestCategory("Sandbox Tests")]
        public void SortingBenchmarkTestListSort() {
            var rg = new McraRandomGenerator();
            var numbers = new List<double>();
            for (int i = 0; i < 1_000_000; i++) {
                numbers.Add(rg.NextDouble());
            }
            var sw = new Stopwatch();
            sw.Start();
            var s2 = numbers.AsEnumerable().Where(v => !double.IsNaN(v)).ToList();
            s2.Sort();
            sw.Stop();
            Trace.WriteLine($"Elapsed: {sw.Elapsed}");
        }

        /// <summary>
        /// Assert that this function is not dependent of the order of the input values
        /// Make sure the input values have duplicates, this is also the case in the observed
        /// values that result in the bug
        /// </summary>
        [TestMethod]
        public void Percentiles_TestPercentilesWithSamplingWeightsSpecificOrder() {
            var equalityComparer = new DoubleAsFloatEqualityComparer();
            var percentages = new double[] { 25, 50, 75, 95, 99.999 };

            var numbers = new double[] { 47, 6, 26, 26, 26 };
            var weights = new double[] { 1, 1, 1, 1, 1 }.ToList();

            var result1 = numbers.PercentilesWithSamplingWeights(weights, percentages);

            //numbers and weights in different order
            numbers = new double[] { 26, 26, 26, 47, 6 };
            weights = new double[] { 1, 1, 1, 1, 1 }.ToList();

            //var result2 = numbers.PercentilesWithSamplingWeights(weights, percentages);
            var result2 = numbers.PercentilesWithSamplingWeights(weights, percentages);

            Assert.IsTrue(result1.Length == result2.Length &&
                          result1.SequenceEqual(result2, equalityComparer));
        }

        /// <summary>
        /// Without zeros
        /// </summary>
        [TestMethod]
        public void Percentiles_TestPercentilesWithSamplingWeightsNoZeros() {
            var equalityComparer = new DoubleAsFloatEqualityComparer();
            var percentages = GriddingFunctions.GetPlotPercentages();
            var numbers = new double[] { 1, 1, 2, 2, 2, 2, 2, 3 };
            var weights = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 }.ToList();
            var result1 = numbers.PercentilesWithSamplingWeights(weights, percentages);
            var result2 = numbers.PercentilesAdditionalZeros(weights, percentages, 0);
            Assert.IsTrue(result1.Length == result2.Length &&
              result1.SequenceEqual(result2, equalityComparer));
        }

        /// <summary>
        /// With zeros and weights
        /// </summary>
        [TestMethod]
        public void Percentiles_TestPercentilesWithSamplingWeightsWithZeros() {
            var equalityComparer = new DoubleAsFloatEqualityComparer();
            var percentages = GriddingFunctions.GetPlotPercentages();
            var numbers = new double[] { 1, 1, 2, 2, 2, 2, 2, 3, 0, 0, 0, 0 };
            var weights = new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }.ToList();
            var result1 = numbers.PercentilesWithSamplingWeights(weights, percentages);
            var numbers1 = new double[] { 1, 1, 2, 2, 2, 2, 2, 3 };
            var weights1 = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 }.ToList();
            var result2 = numbers1.PercentilesAdditionalZeros(weights1, percentages, 4);
            Assert.IsTrue(result1.Length == result2.Length &&
              result1.SequenceEqual(result2, equalityComparer));
        }

        /// <summary>
        /// With zeros and weights
        /// </summary>
        [TestMethod]
        public void Percentiles_TestPercentilesWithSamplingWeightsWithZeros1() {
            var equalityComparer = new DoubleAsFloatEqualityComparer();
            var percentages = GriddingFunctions.GetPlotPercentages();
            var numbers = new double[] { 1, 1, 2, 2, 2, 2, 2, 3, 0 };
            var weights = new double[] { 1, 1, 1, 1, 1, 1, 1, 1, .1 }.ToList();
            var result1 = numbers.PercentilesWithSamplingWeights(weights, percentages);
            var numbers1 = new double[] { 1, 1, 2, 2, 2, 2, 2, 3 };
            var weights1 = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 }.ToList();
            var result2 = numbers1.PercentilesAdditionalZeros(weights1, percentages, .1);
            Assert.IsTrue(result1.Length == result2.Length &&
              result1.SequenceEqual(result2, equalityComparer));
        }

        /// <summary>
        /// Assert that percentiles computed with zeros specified as the number
        /// number of records being zero are equal to percentiles with zeros added
        /// to the dataset. Test is performed for a number of times on randomly
        /// generated value series.
        /// </summary>
        [TestMethod]
        public void Percentiles_TestPercentilesWithSamplingWeightsWithZerosRandom() {
            var equalityComparer = new DoubleAsFloatEqualityComparer();
            var percentages = GriddingFunctions.GetPlotPercentages();
            var numTests = 100;
            for (int i = 0; i < numTests; i++) {
                var n = 5000;
                var numZeros = 100;
                
                var random = new McraRandomGenerator(i);
                var logNormal = new LogNormalDistribution(0, 1);
                var valuesNoZeros = logNormal.Draws(random, n);

                var continuousUniform = new ContinuousUniformDistribution(0, 1);
                var weightsNoZeroes = continuousUniform.Draws(random, n);

                var valuesWithZeros = valuesNoZeros.Concat(Enumerable.Repeat(0D, numZeros)).ToList();
                var weightsWithZeroes = weightsNoZeroes.Concat(Enumerable.Repeat(1D, numZeros)).ToList();

                var result1 = valuesWithZeros.PercentilesWithSamplingWeights(weightsWithZeroes, percentages);
                var result2 = valuesNoZeros.PercentilesAdditionalZeros(weightsNoZeroes, percentages, numZeros);

                Assert.IsTrue(result1.Length == result2.Length && result1.SequenceEqual(result2, equalityComparer));
            }
        }

        /// <summary>
        /// With zeros and weights
        /// </summary>
        [TestMethod]
        public void Percentiles_TestPercentilesWithSamplingWeightsWithZeros2() {
            var equalityComparer = new DoubleAsFloatEqualityComparer();
            var percentages = GriddingFunctions.GetPlotPercentages();
            var numbers = new double[] { 1, 1, 2, 2, 2, 2, 2, 3, 0, 0 };
            var weights = new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, .1 }.ToList();
            var result1 = numbers.PercentilesWithSamplingWeights(weights, percentages);
            var numbers1 = new double[] { 1, 1, 2, 2, 2, 2, 2, 3 };
            var weights1 = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 }.ToList();
            var result2 = numbers1.PercentilesAdditionalZeros(weights1, percentages, 1.1);
            Assert.IsTrue(result1.Length == result2.Length &&
              result1.SequenceEqual(result2, equalityComparer));
        }

        /// <summary>
        /// PercentilesTest
        /// </summary>
        [TestMethod]
        public void Percentiles_TestPercentiles() {
            var percentages = new double[] { 25, 50, 75, 95 };
            var equalityComparer = new DoubleAsFloatEqualityComparer();

            var runTest = new Func<double[], double[], bool>((n, expected) => {
                var result = n.Percentiles(percentages);
                //Compare the resulting arrays using SequenceEqual with the equalitycomparer
                //that converts to float, because double precision is not needed here
                Assert.IsTrue(expected.Length == result.Length && expected.SequenceEqual(result, equalityComparer),
                              $"Comparing percentile ranges [{string.Join(", ", expected)}] and [{string.Join(", ", result)}]");

                return true;
            });

            runTest(new double[] { }, new double[] { 0, 0, 0, 0 });
            runTest(new double[] { 1, 1, 2, 2, 2, 2, 2, 3 }, new double[] { 1, 2, 2, 2.6 });
            runTest(new double[] { 10, 10, 20, 20, 20, 20, 20, 30 }, new double[] { 10, 20, 20, 26 });
            runTest(new double[] { 100, 100, 200, 200, 200, 200, 200, 300 }, new double[] { 100, 200, 200, 260 });
            runTest(new double[] { 9, 8, 7, 6, 0, 0, 0, 0, 0, 0 }, new double[] { 0, 0, 6.5, 8.5 });

            var numbers = new double[] { 9, 8, 7, 6, 0, 0, 0, 0, 0, 0 };
            var weights = new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }.ToList();
            var result1 = numbers.PercentilesWithSamplingWeights(weights, percentages);

            var numbers1 = new double[] { 9, 8, 7, 6 };
            var weights1 = new double[] { 1, 1, 1, 1 }.ToList();

            var result2 = numbers1.PercentilesAdditionalZeros(weights1, percentages, 6);
            Assert.IsTrue(result1.Length == result2.Length &&
              result1.SequenceEqual(result2, equalityComparer));
        }

        /// <summary>
        /// PercentilesWithSamplingWeightsTest, This example gives a bug when some weights are zero. 
        /// It is easy to adapt the algorithm for zeros weights, but apparently it never happens.
        /// Note that samples may contain zero weights, it depends if the percentile is in this area whether an error occurs or not.
        /// </summary>
        [TestMethod]
        public void Percentiles_ZeroWeights() {
            var percentages = new double[] { 50 };
            var numbers = new double[] { 1,2,3,4 };
            //var weights = Enumerable.Repeat(1d, 2).ToList();
            var weights = new double[] { 1,0,1,0 }.ToList();
            var result = numbers.PercentilesWithSamplingWeights(weights, percentages);
        }



        /// <summary>
        /// PercentilesWithSamplingWeightsTest
        /// </summary>
        [TestMethod]
        public void Percentiles_PercentilesWithSamplingWeightsTest() {
            var percentages = new double[] { 25, 50, 75, 95 };
            var equalityComparer = new DoubleAsFloatEqualityComparer();

            var runTest = new Func<double[], double[], double[], bool>((n, w, expected) => {
                var result = n.PercentilesWithSamplingWeights(w.ToList(), percentages);
                //Compare the resulting arrays using SequenceEqual with the equalitycomparer
                //that converts to float, because double precision is not needed here
                Assert.IsTrue(expected.Length == result.Length && expected.SequenceEqual(result, equalityComparer),
                              $"Comparing percentile ranges [{string.Join(", ", expected)}] and [{string.Join(", ", result)}]");

                return true;
            });

            var expectedPercentiles = new double[] { 1, 2, 2, 2.6 };
            runTest(new double[] { 1, 1, 2, 2, 2, 2, 2, 3 }, new double[] { 1, 1, 1, 1, 1, 1, 1, 1 }, expectedPercentiles);
            runTest(new double[] { }, new double[] { }, new double[] { 0, 0, 0, 0 });

            expectedPercentiles = new double[] { 1, 1.4, 1.8, 2.6 };
            runTest(new double[] { 1, 2, 3 }, new double[] { 2, 5, 1 }, expectedPercentiles);
            runTest(new double[] { 1, 2, 3 }, new double[] { 1, 2.5, 0.5 }, expectedPercentiles);
            runTest(new double[] { 1, 2, 3 }, new double[] { 1, 2.5, 0.5 }, expectedPercentiles);
            runTest(new double[] { 10, 20, 30 }, new double[] { 2, 5, 1 }, new double[] { 10, 14, 18, 26 });
            runTest(new double[] { 10, 20, 30 }, new double[] { 20, 50, 10 }, new double[] { 10, 14, 18, 26 });
            runTest(new double[] { 100, 200, 300 }, new double[] { 20, 50, 10 }, new double[] { 100, 140, 180, 260 });
            runTest(new double[] { }, new double[] { }, new double[] { 0, 0, 0, 0 });
        }

        /// <summary>
        /// Assert that this function is not dependent of the order of the input values
        /// Make sure the input values have duplicates, this is also the case in the observed
        /// values that result in the bug
        /// </summary>
        [TestMethod]
        public void Percentiles_PercentilesWithSamplingWeightsRandomOrderTest() {
            var equalityComparer = new DoubleAsFloatEqualityComparer();
            var percentages = new double[] { 25, 50, 75, 95 };

            //pick a random seed
            var seed = Environment.TickCount;
            var rg = new McraRandomGenerator(seed);

            for (var a = 0; a < 1000; a++) {
                //array with doubles to choose from
                var choice = rg.Next(2, 10);
                //amount of doubles in the numbers/weights arrays
                var amount = rg.Next(1, 15);
                var numbers = new double[amount];
                var weights = new double[amount];
                var order = new int[amount];

                //fill the arrays with the values to choose from
                var nDoubles = new double[choice];
                var wDoubles = new double[choice];
                for (var i = 0; i < choice; i++) {
                    nDoubles[i] = rg.Next(1, 50) * rg.NextDouble();
                    wDoubles[i] = 0.5D * rg.NextDouble();
                }

                //now fill the input arrays with the numbers and weights to test
                //the restriction on the amount of doubles to choose from 
                //results in duplicates in the numbers and weights arrays
                for (int i = 0; i < amount; i++) {
                    numbers[i] = nDoubles[rg.Next(choice)];
                    weights[i] = wDoubles[rg.Next(choice)];
                }

                var compare = numbers.PercentilesWithSamplingWeights(weights.ToList(), percentages);
                Debug.WriteLine("number sequence " + a + " percentiles: " + string.Join(",", compare));
                Debug.WriteLine(" - " + string.Join(" ", numbers.Zip(weights, (n, w) => $"n,w: {n},{w}")));

                for (var t = 0; t < amount * 10; t++) {
                    //shuffle the numbers in the arrays of numbers and weights, 
                    //but keep numbers and weights together
                    for (var i = amount - 1; i > 0; i--) {
                        // Swap element "i" with a random earlier element it (or itself)
                        var swapIndex = rg.Next(i + 1);
                        var tmpNumber = numbers[i];
                        var tmpWeight = weights[i];
                        numbers[i] = numbers[swapIndex];
                        weights[i] = weights[swapIndex];
                        numbers[swapIndex] = tmpNumber;
                        weights[swapIndex] = tmpWeight;
                    }
                    var result = numbers.PercentilesWithSamplingWeights(weights.ToList(), percentages);
                    Debug.WriteLine("shuffle " + t + " percentiles: " + string.Join(",", result));
                    Debug.WriteLine(string.Join(" ", numbers.Zip(weights, (n, w) => $"n,w: {n},{w}")));

                    Assert.IsTrue(compare.Length == result.Length && compare.SequenceEqual(result, equalityComparer),
                        $"Random seed {seed}, comparing percentile ranges [{string.Join(", ", compare)}] and [{string.Join(", ", result)}]");
                }
            }
        }
    }
}
