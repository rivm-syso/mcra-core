using MCRA.Utils.Statistics.RandomGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {

    /// <summary>
    /// Test methods for the <see cref="RandomUtils"/> class.
    /// </summary>
    [TestClass]
    public class RandomUtilsTests {

        /// <summary>
        /// Assert that the create seed method returns the same seed when no
        /// hash codes are provided.
        /// </summary>
        [TestMethod]
        public void RandomUtils_TestCreateSeed_Empty() {
            var rnd = new Random(123456);
            var seeds = createSeeds(rnd, 1000);
            foreach (var baseSeed in seeds) {
                var seed = RandomUtils.CreateSeed(baseSeed, new int[] { });
                Assert.AreEqual(baseSeed, seed);
            }
        }

        /// <summary>
        /// Test create seed method for a number of random seeds and single hash 
        /// codes. Assert that the collision ratio is acceptable.
        /// </summary>
        [TestMethod]
        public void RandomUtils_TestCreateSeed_OneCollisions() {
            // Create a collection of seeds including some specific values
            var rnd = new Random(123456);
            var seeds = createSeeds(rnd, 1000);

            // Create a collection of pairs to test with, including some specific combinations
            var codes = new[] { int.MinValue, 0, int.MaxValue }.ToList();
            codes = codes
                .Union(
                    Enumerable.Range(0, 1000)
                        .Select(r => rnd.Next(int.MinValue, int.MaxValue))
                        .Distinct()
                )
                .ToList();
            codes.Sort();
 
            // Loop over seeds
            var totalCollisionCount = 0;
            var totalCount = 0;
            var totalTabuList = new HashSet<int>();
            for (int i = 0; i < seeds.Count; i++) {
                var baseSeed = seeds[i];

                // Loop over code pairs for this seed
                var innerCollisionCount = 0;
                var innerCount = 0;
                var innerTabuList = new HashSet<int>();
                for (int j = 0; j < codes.Count; j++) {
                    var code = codes[j];
                    var seed = RandomUtils.CreateSeed(baseSeed, code);

                    // Assert that using the same code twice does not generate the same seed
                    // accept some weird behavour when base-seed or code is 0
                    if (baseSeed != 0 && code != 0) {
                        var seedRepeat = RandomUtils.CreateSeed(baseSeed, code, code);
                        Assert.AreNotEqual(seed, seedRepeat);
                    }

                    // Update inner count and tabu list
                    if (innerTabuList.Contains(seed)) {
                        innerCollisionCount++;
                    } else {
                        innerTabuList.Add(seed);
                    }
                    innerCount++;

                    // Update total count and tabu list
                    if (totalTabuList.Contains(seed)) {
                        totalCollisionCount++;
                    } else {
                        totalTabuList.Add(seed);
                    }
                    totalCount++;
                }

                // Collision within this seed
                var innerCollisionRate = (double)innerCollisionCount / innerCount;
                Assert.IsTrue(innerCollisionRate < 0.01);
            }

            // Collisions over all seeds
            var collisionRate = (double)totalCollisionCount / totalCount;
            Assert.IsTrue(collisionRate < 0.001);
        }

        /// <summary>
        /// Test create seed method for a number of random seeds and tuples of hash 
        /// codes. Assert that the collision ratio is acceptable.
        /// </summary>
        [TestMethod]
        public void RandomUtils_TestCreateSeed_TwoCollisions() {
            var rnd = new Random(123456);

            // Create a collection of seeds including some specific values
            var seeds = createSeeds(rnd, 1000);

            // Create a collection of pairs to test with, including some specific combinations
            var codepairs = new[] {
                (int.MinValue, int.MinValue),
                (0, 0),
                (int.MaxValue, int.MaxValue)
            }.ToList();
            codepairs = codepairs
                .Union(
                    Enumerable.Range(0, 1000)
                        .Select(r => (rnd.Next(int.MinValue, int.MaxValue), rnd.Next(int.MinValue, int.MaxValue)))
                        .Distinct()
                )
                .ToList();
            codepairs = codepairs.Union(codepairs.Select(r => (r.Item2, r.Item1))).Distinct().ToList();

            // Loop over seeds
            var totalCollisionCount = 0;
            var totalCount = 0;
            var totalTabuList = new HashSet<int>();
            for (int i = 0; i < seeds.Count; i++) {
                var baseSeed = seeds[i];

                // Loop over code pairs for this seed
                var innerCollisionCount = 0;
                var innerCount = 0;
                var innerTabuList = new HashSet<int>();
                for (int j = 0; j < codepairs.Count; j++) {
                    var code = codepairs[j];
                    var seed = RandomUtils.CreateSeed(baseSeed, code.Item1, code.Item2);
                    if (code.Item1 != code.Item2) {
                        // If item1 and item2 are not equal, then the seed generated with the
                        // inverse order should not be equal
                        var seedInv = RandomUtils.CreateSeed(baseSeed, code.Item2, code.Item1);
                        Assert.AreNotEqual(seed, seedInv);
                    }

                    // Update inner count and tabu list
                    if (innerTabuList.Contains(seed)) {
                        innerCollisionCount++;
                    } else {
                        innerTabuList.Add(seed);
                    }
                    innerCount++;

                    // Update total count and tabu list
                    if (totalTabuList.Contains(seed)) {
                        totalCollisionCount++;
                    } else {
                        totalTabuList.Add(seed);
                    }
                    totalCount++;
                }

                // Collision within this seed
                var innerCollisionRate = (double)innerCollisionCount / innerCount;
                Assert.IsTrue(innerCollisionRate < 0.01);
            }

            // Collisions over all seeds
            var collisionRate = (double)totalCollisionCount / totalCount;
            Assert.IsTrue(collisionRate < 0.001);
        }

        private static List<int> createSeeds(Random rnd, int numSeeds) {
            var seeds = new[] { int.MinValue, -123456, -12345, -1, 0, 1, 12345, 123456, int.MaxValue }.ToList();
            seeds = seeds.Union(Enumerable.Range(0, (numSeeds - 8) / 2).Select(r => rnd.Next())).ToList();
            seeds = seeds.Union(Enumerable.Range(0, (numSeeds - 8) / 2).Select(r => -rnd.Next())).ToList();
            seeds.Sort();
            return seeds;
        }
    }
}
