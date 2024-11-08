using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.McraRandomGenerators {
    [TestClass]
    public class McraRandomGeneratorTests {

        /// <summary>
        /// Test equal seeds
        /// </summary>
        [TestMethod]
        public void McraRandomGenerator_TestSeedsEqual() {
            var random1 = new McraRandomGenerator(1);
            var random2 = new McraRandomGenerator(1);
            Assert.AreEqual(random1.NextDouble(), random2.NextDouble());
        }

        /// <summary>
        /// Test different seeds
        /// </summary>
        [TestMethod]
        public void McraRandomGenerator_TestSeedsDifferent() {
            var random1 = new McraRandomGenerator(1);
            var random2 = new McraRandomGenerator(2);
            Assert.IsTrue(random1.NextDouble() != random2.NextDouble());
        }

        /// <summary>
        /// Test equal draws
        /// </summary>
        [TestMethod]
        public void McraRandomGenerator_TestReplay() {
            var random = new McraRandomGenerator(new int[] { 1234, 5678 });
            var draws1 = Enumerable.Range(0, 10).Select(c => c = random.Next()).ToArray();
            random.Reset();
            var draws2 = Enumerable.Range(0, 10).Select(c => c = random.Next()).ToArray();
            CollectionAssert.AreEqual(draws1, draws2);
        }

        /// <summary>
        /// Test multiple seeds
        /// </summary>
        /// <param name="seeds"></param>
        [TestMethod]
        [DataRow(new int[] { int.MaxValue, int.MaxValue, 3 })]
        [DataRow(new int[] { int.MinValue, int.MinValue, 0 })]
        [DataRow(new int[] { int.MinValue, int.MaxValue, 0 })]
        [DataRow(new int[] { 0, int.MinValue, int.MaxValue })]
        [DataRow(new int[] { int.MaxValue, int.MinValue })]
        [DataRow(new int[] { 1, 1, 1 })]
        [DataRow(new int[] { 0, 1, 0 })]
        [DataRow(new int[] { 0, 0, 0 })]
        public void McraRandomGenerator_TestMultipleSeeds(int[] seeds) {
            var random = new McraRandomGenerator(seeds);
            Assert.IsFalse(seeds.Contains(random.Seed));
        }

        /// <summary>
        /// Test unequal seeds
        /// </summary>
        /// <param name="seeds1"></param>
        /// <param name="seeds2"></param>
        [TestMethod]
        [DataRow(new int[] { 1, 2, 3 }, new int[] { 3, 2, 1 })]
        [DataRow(new int[] { 1, 2, 3 }, new int[] { 2, 2, 3 })]
        [DataRow(new int[] { 0, 1 }, new int[] { 0, 0, 1 })]
        [DataRow(new int[] { -1, 1 }, new int[] { 1, -1 })]
        public void McraRandomGenerator_TestUnEqualSeeds(int[] seeds1, int[] seeds2) {
            var random1 = new McraRandomGenerator(seeds1);
            var random2 = new McraRandomGenerator(seeds2);
            Assert.IsFalse(random1.Seed == random2.Seed);
        }

        /// <summary>
        /// Test full range, minimum and maximum
        /// </summary>
        [TestMethod]
        public void McraRandomGenerator_TestMinMaxSeed() {
            var random = new Random(1);
            var seeds = new List<int>();
            for (int i = 0; i < 1000; i++) {
                var drawSeeds = Enumerable.Range(0, 3).Select(c => c = random.Next()).ToArray();
                var rnd = new McraRandomGenerator(drawSeeds);
                seeds.Add(rnd.Seed);
            }
            seeds.Sort();
            var min = seeds.Min();
            var max = seeds.Max();
            Assert.IsTrue(min < short.MinValue);
            Assert.IsTrue(max > short.MaxValue);

        }
    }
}
