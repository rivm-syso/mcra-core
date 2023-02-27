using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock substance residue collections
    /// </summary>
    public static class MockCompoundResidueCollectionsGenerator {
        /// <summary>
        /// Creates  substance residues collections
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="sampleCompoundCollections"></param>
        /// <returns></returns>
        public static IDictionary<(Food Food, Compound Substance), CompoundResidueCollection> Create(
            ICollection<Compound> substances,
            IDictionary<Food, SampleCompoundCollection> sampleCompoundCollections
        ) {
            var compoundResidueCollectionsBuilder = new CompoundResidueCollectionsBuilder(false);
            return compoundResidueCollectionsBuilder.Create(substances, sampleCompoundCollections.Values, null, null);
        }

        /// <summary>
        /// Creates concentration models.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="substances"></param>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <param name="ractionZero"></param>
        /// <param name="lors"></param>
        /// <param name="markZerosAsNonDetects"></param>
        /// <param name="sampleSize"></param>
        /// <returns></returns>
        public static IDictionary<(Food, Compound), CompoundResidueCollection> Create(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            double mu = double.NaN,
            double sigma = double.NaN,
            double ractionZero = double.NaN,
            double[] lors = null,
            bool markZerosAsNonDetects = false,
            int sampleSize = -1
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var result = new Dictionary<(Food, Compound), CompoundResidueCollection>();
            foreach (var food in foods) {
                var foodSampleSize = sampleSize > 0 ? sampleSize : random.Next(1, 100);
                foreach (var substance in substances) {
                    CompoundResidueCollection record = CreateSingle(
                        food,
                        substance,
                        mu,
                        sigma,
                        ractionZero,
                        markZerosAsNonDetects,
                        lors,
                        foodSampleSize,
                        random.Next());
                    result[(food, substance)] = record;
                }
            }
            return result;
        }

        /// <summary>
        /// Creates a single compound residue collection.
        /// </summary>
        /// <param name="food"></param>
        /// <param name="substance"></param>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <param name="fractionZero"></param>
        /// <param name="markZerosAsNonDetects"></param>
        /// <param name="lors"></param>
        /// <param name="numberOfSamples"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static CompoundResidueCollection CreateSingle(
            Food food,
            Compound substance,
            double mu,
            double sigma,
            double fractionZero,
            bool markZerosAsNonDetects,
            double[] lors,
            int numberOfSamples,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var concentrations = createConcentrations(
                !double.IsNaN(mu) ? mu : -1 + 2 * random.NextDouble(),
                !double.IsNaN(sigma) ? sigma : .1 + .9 * random.NextDouble(),
                !double.IsNaN(fractionZero) ? fractionZero : random.NextDouble(),
                numberOfSamples,
                random
            );
            lors = lors ?? new double[] { .005, .01, .05, .1 };
            var curLor = lors[random.Next(lors.Length)];
            var positivesCount = concentrations.Where(r => r > 0).Count();
            var zerosCount = markZerosAsNonDetects ? 0 : concentrations.Where(r => r == 0).Count();
            var nonDetectsCollection1 = concentrations.Where(r => r < curLor)
                .Select(r => new CensoredValueCollection() { LOD = curLor, LOQ = curLor })
                .ToList();
            var nonDetectsCollection2 = concentrations.Where(r => r > 0 && r < curLor)
                .Select(r => new CensoredValueCollection() { LOD = curLor, LOQ = curLor })
                .ToList();
            var record = new CompoundResidueCollection() {
                Food = food,
                Compound = substance,
                Positives = concentrations.Where(r => r >= curLor).ToList(),
                CensoredValuesCollection = markZerosAsNonDetects
                    ? nonDetectsCollection1
                    : nonDetectsCollection2,
                ZerosCount = zerosCount,

            };
            return record;
        }

        /// <summary>
        /// Creates concentrations based on mu and sigmas
        /// </summary>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <param name="fractionZero"></param>
        /// <param name="n"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static List<double> createConcentrations(
                double mu,
                double sigma,
                double fractionZero,
                int n,
                IRandom random
            ) {
            var positives = (int)(n - Math.Round(fractionZero * n));
            var zeros = n - positives;
            var x = Enumerable
                .Range(0, n)
                .Select(r => r < positives ? LogNormalDistribution.InvCDF(mu, sigma, random.NextDouble(0, 1)) : 0D)
                .ToList();
            return x.Shuffle(random).ToList();
        }
    }
}
