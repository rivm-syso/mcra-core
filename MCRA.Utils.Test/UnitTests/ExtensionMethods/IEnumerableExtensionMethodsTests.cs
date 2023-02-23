using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    [TestClass]
    public class IEnumerableExtensionMethodsTests {

        [TestMethod]
        public void WeightedPercentages() {
            var percentiles = new double[] { .2, 1, 2, 3, 4, 5, 1.1, 2.3, 3.4, 3.9 };
            var limits = percentiles.AsEnumerable();
            var w = new List<double>() { 1, 1, 1, 1.001 };
            var xValues = new List<double>() { 1, 2, 3, 4 };

            var test1 = Stats.PercentagesWithSamplingWeights(xValues, w, limits);
            var test = Stats.Percentages(xValues, limits);
        }

        [TestMethod]
        public void WeightedPercentiles() {
            var percentages = new double[] { 10, 20, 50, 75, 90, 95, 99 };

            var w = new List<double>() { 1 };
            var xValues = new List<double>() { 1 };
            var test1 = Stats.PercentilesWithSamplingWeights(xValues, w, 77);
            var test = Stats.PercentilesWithSamplingWeights(xValues, w, percentages);

            w = new List<double>() { 1, 3 };
            xValues = new List<double>() { 3, 1 };
            test = Stats.PercentilesWithSamplingWeights(xValues, w, percentages);


            w = new List<double>() { 1, 3, 4, 4, 4, 20 };
            xValues = new List<double>() { 1, 4, 6, 7, 8, 10 };
            test = Stats.PercentilesWithSamplingWeights(xValues, w, percentages);
        }

        [TestMethod]
        public void ResampleTest1() {
            var a = new List<double>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var b = a.Resample();
            Assert.IsTrue(a.Count == b.Count());
        }

        [TestMethod]
        public void PartitionTest1() {
            var a = new List<double>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            Assert.IsTrue(a.Partition(3).Count() == 4);
            Assert.IsTrue(a.Partition(2).Count() == 5);
            Assert.IsTrue(a.Partition(30).Count() == 1);
        }

        [TestMethod]
        public void PartitionTest2() {
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
        public void FullSelfIntersectTest1() {
            var sets = new List<List<double>>();
            sets.Add(new List<double>() { 1, 2, 3, 4, 5 });
            sets.Add(new List<double>() { 1, 2, 4, 5 });
            sets.Add(new List<double>() { 5 });
            var actual = sets.FullSelfIntersect();
            Assert.IsTrue(actual.Count() == 1);
            Assert.AreEqual(actual.Single(), 5);
        }

        [TestMethod]
        public void AverageOrZeroTest1() {
            var l = new List<double>();
            Assert.IsTrue(l.AverageOrZero() == 0);
        }

        [TestMethod]
        public void DrawRandomTest1() {
            var l = new List<double>() { 10, 50, 20, 05, 15 };
            var n = 1000000;

            var pExtractor = new Func<double, double>((v) => v / 100);

            var random = new McraRandomGenerator();

            var results = new double[n];
            for (int i = 0; i < n; i++) {
                results[i] = l.DrawRandom(random as IRandom, pExtractor);
            }

            System.Diagnostics.Trace.WriteLine(results.Count(v => v == 10) / (double)n);
            System.Diagnostics.Trace.WriteLine(results.Count(v => v == 50) / (double)n);
            System.Diagnostics.Trace.WriteLine(results.Count(v => v == 20) / (double)n);
            System.Diagnostics.Trace.WriteLine(results.Count(v => v == 5) / (double)n);
            System.Diagnostics.Trace.WriteLine(results.Count(v => v == 15) / (double)n);
        }

        [TestMethod]
        public void DrawRandomTest2() {
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
                results[i] = l.DrawRandom(random as IRandom);
            }

            sw.Stop();
            System.Diagnostics.Trace.WriteLine(message: $"Elapsed: {sw.Elapsed}");
        }

        [TestMethod]
        public void DrawRandomTest3() {
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
        public void VarianceTest1() {
            var numbers = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, double.NaN };
            Assert.IsTrue(numbers.Variance() == numbers.AsEnumerable().Variance());
        }

        [TestMethod]
        public void SelectCombineTest1() {
            var employees = new List<Employee>() {
                new Employee() {
                    Name = "Fons",
                },
                new Employee() {
                    Name = "Ellen",
                },
                new Employee() {
                    Name = "Jim",
                },
                new Employee() {
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
        public void GetPatternIdTest2() {
            var l = new string[] { "A", "B", "C" };
            var p1 = l.GetPatternId(s => !string.IsNullOrEmpty(s));
            Assert.IsTrue(p1 == 7);
        }

        [TestMethod]
        public void GetPatternIdTest3() {
            var l = new string[] { "", "B", "C" };
            var p1 = l.GetPatternId(s => !string.IsNullOrEmpty(s));
            Assert.IsTrue(p1 == 6);
        }

        [TestMethod]
        public void GetPatternIdTest4() {
            var l = new string[] { "A", "", "C" };
            var p1 = l.GetPatternId(s => !string.IsNullOrEmpty(s));
            Assert.IsTrue(p1 == 5);
        }
    }
}
