using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    [TestClass]
    public class IEnumerableExtensionsTests {

        [TestMethod]
        public void IEnumerableExtensions_WeightedPercentages() {
            var percentiles = new double[] { .2, 1, 2, 3, 4, 5, 1.1, 2.3, 3.4, 3.9 };
            var limits = percentiles.AsEnumerable();
            var w = new List<double>() { 1, 1, 1, 1.001 };
            var xValues = new List<double>() { 1, 2, 3, 4 };

            var test1 = Stats.PercentagesWithSamplingWeights(xValues, w, limits);
            var test = Stats.Percentages(xValues, limits);
        }

        [TestMethod]
        public void IEnumerableExtensions_WeightedPercentiles() {
            var percentages = new double[] { 10, 20, 50, 75, 90, 95, 99 };

            var w = new List<double>() { 1 };
            var xValues = new List<double>() { 1 };
            var test1 = Stats.PercentilesWithSamplingWeights(xValues, w, 77);
            var test = Stats.PercentilesWithSamplingWeights(xValues, w, percentages);

            w = [1, 3];
            xValues = [3, 1];
            test = Stats.PercentilesWithSamplingWeights(xValues, w, percentages);


            w = [1, 3, 4, 4, 4, 20];
            xValues = [1, 4, 6, 7, 8, 10];
            test = Stats.PercentilesWithSamplingWeights(xValues, w, percentages);
        }

        [TestMethod]
        public void IEnumerableExtensions_ResampleTest1() {
            var a = new List<double>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var b = a.Resample();
            Assert.HasCount(b.Count(), a);
        }

        [TestMethod]
        public void IEnumerableExtensions_PartitionTest1() {
            var a = new List<double>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            Assert.AreEqual(4, a.Partition(3).Count());
            Assert.AreEqual(5, a.Partition(2).Count());
            Assert.AreEqual(1, a.Partition(30).Count());
        }

        [TestMethod]
        public void IEnumerableExtensions_PartitionTest2() {
            var totalSize = 1000000;
            var partitionSize = 1000;
            var allEmployees = new List<Employee>(totalSize);
            for (int i = 0; i < totalSize; ++i) {
                allEmployees.Add(
                    new Employee() { Name = $"Emp {i}", Age = 40 }
                );
            };
            var partitions = allEmployees.Partition(1000);
            Assert.AreEqual(totalSize / partitionSize, partitions.Count());
        }

        [TestMethod]
        public void IEnumerableExtensions_FullSelfIntersectTest1() {
            var sets = new List<List<double>> {
                new() { 1, 2, 3, 4, 5 },
                new() { 1, 2, 4, 5 },
                new() { 5 }
            };
            var actual = sets.FullSelfIntersect();
            Assert.AreEqual(1, actual.Count());
            Assert.AreEqual(5, actual.Single());
        }

        [TestMethod]
        public void IEnumerableExtensions_AverageOrZeroTest1() {
            var l = new List<double>();
            Assert.AreEqual(0, l.AverageOrZero());
        }

        [TestMethod]
        public void IEnumerableExtensions_DrawRandomTest1() {
            var l = new List<double>() { 10, 50, 20, 05, 15 };
            var n = 1000000;

            var pExtractor = new Func<double, double>((v) => v / 100);

            var random = new McraRandomGenerator();

            var results = new double[n];
            for (int i = 0; i < n; i++) {
                results[i] = l.DrawRandom(random, pExtractor);
            }

            System.Diagnostics.Trace.WriteLine(results.Count(v => v == 10) / (double)n);
            System.Diagnostics.Trace.WriteLine(results.Count(v => v == 50) / (double)n);
            System.Diagnostics.Trace.WriteLine(results.Count(v => v == 20) / (double)n);
            System.Diagnostics.Trace.WriteLine(results.Count(v => v == 5) / (double)n);
            System.Diagnostics.Trace.WriteLine(results.Count(v => v == 15) / (double)n);
        }

        [TestMethod]
        public void IEnumerableExtensions_DrawRandomTest2() {
            var l = new List<double>();
            var n = 1000000;

            var random = new McraRandomGenerator();

            for (int i = 0; i < 6000; i++) {
                l.Add(random.NextDouble());
            }

            var results = new double[n];

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < n; i++) {
                results[i] = l.DrawRandom(random);
            }

            sw.Stop();
            System.Diagnostics.Trace.WriteLine(message: $"Elapsed: {sw.Elapsed}");
        }

        [TestMethod]
        public void IEnumerableExtensions_DrawRandomTest3() {
            var l = new List<double>();
            var n = 1000000;

            var random = new McraRandomGenerator();

            for (int i = 0; i < 6000; i++) {
                l.Add(random.NextDouble());
            }

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var results = l.DrawRandom(random, n);

            sw.Stop();
            System.Diagnostics.Trace.WriteLine($"Elapsed: {sw.Elapsed}");
        }

        [TestMethod]
        public void IEnumerableExtensions_VarianceTest1() {
            var numbers = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, double.NaN };
            Assert.AreEqual(numbers.AsEnumerable().Variance(), numbers.Variance());
        }

        [TestMethod]
        public void IEnumerableExtensions_SelectCombineTest1() {
            var employees = new List<Employee>() {
                new() {
                    Name = "Fons",
                },
                new() {
                    Name = "Ellen",
                },
                new() {
                    Name = "Jim",
                },
                new() {
                    Name = "Jaap",
                },
            };
            var combinations = employees
                .SelectCombine((emp1, emp2) => new Tuple<Employee, Employee>(emp1, emp2))
                .ToList();

            foreach (var c in combinations) {
                System.Diagnostics.Trace.WriteLine(c);
            }
        }

        [TestMethod]
        [DataRow(new int[] { 0, 1, 2, 1 }, 1, 1)]
        [DataRow(new int[] { 0, 1, 2, 1 }, 3, -1)]
        [DataRow(new int[] { 0, 1, 2, 1 }, 2, 2)]
        [DataRow(new int[] { }, 2, -1)]
        public void IEnumerableExtensions_FirstIndexMatch(
            int[] sequence,
            int value,
            int expeced
        ) {
            var result = sequence.FirstIndexMatch(r => r == value);
            Assert.AreEqual(expeced, result);
        }
    }
}
