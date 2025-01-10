using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock substance residue collections
    /// </summary>
    public static class FakeCompoundResidueCollectionsGenerator {
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
            var compoundResidueCollectionsBuilder = new CompoundResidueCollectionsBuilder();
            return compoundResidueCollectionsBuilder.Create(substances, sampleCompoundCollections.Values, null, null);
        }

        /// <summary>
        /// Creates concentration models.
        /// </summary>
        public static IDictionary<(Food, Compound), CompoundResidueCollection> Create(
            ICollection<Food> foods,
            ICollection<Compound> substances,
            double mean = double.NaN,
            double upper = double.NaN,
            double fractionZero = double.NaN,
            bool treatZerosAsCensored = true,
            double[] lods = null,
            double[] loqs = null,
            int sampleSize = -1
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var logNormalDistribution = LogNormalDistribution.FromMeanAndUpper(mean, upper);

            var result = new Dictionary<(Food, Compound), CompoundResidueCollection>();
            foreach (var food in foods) {
                var foodSampleSize = sampleSize > 0 ? sampleSize : random.Next(1, 100);
                foreach (var substance in substances) {
                    CompoundResidueCollection record = CreateSingle(
                        food,
                        substance,
                        logNormalDistribution.Mu,
                        logNormalDistribution.Sigma,
                        fractionZero,
                        treatZerosAsCensored,
                        lods,
                        loqs,
                        foodSampleSize,
                        random.Next()
                    );
                    result[(food, substance)] = record;
                }
            }
            return result;
        }

        /// <summary>
        /// Creates a single compound residue collection.
        /// </summary>
        public static CompoundResidueCollection CreateSingle(
            Food food,
            Compound substance,
            double mu,
            double sigma,
            double fractionZero,
            bool treatZerosAsCensored,
            double[] lods,
            double[] loqs,
            int numberOfSamples,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);

            lods = lods ?? [.005, .01];
            loqs = loqs ?? [.01, .02];

            var concentrations = createConcentrations(
                !double.IsNaN(mu) ? mu : -1 + 2 * random.NextDouble(),
                !double.IsNaN(sigma) ? sigma : .1 + .9 * random.NextDouble(),
                !double.IsNaN(fractionZero) ? fractionZero : random.NextDouble(),
                numberOfSamples,
                random
            );

            var positives = new List<double>();
            var zerosCount = 0;
            var censoredValuesCollection = new List<CensoredValue>();
            for (int i = 0; i < concentrations.Count; i++) {
                var concentration = concentrations[i];
                var lorIndex = random.Next(lods.Length);
                var lod = lods[lorIndex];
                var loq = loqs[lorIndex];
                if (concentration > loq) {
                    positives.Add(concentration);
                } else if (concentration == 0 && !treatZerosAsCensored) {
                    zerosCount++;
                } else {
                    var censoredValue = new CensoredValue() {
                        LOD = lod,
                        LOQ = loq,
                        ResType = concentration > lod ? ResType.LOQ : ResType.LOD,
                    };
                    censoredValuesCollection.Add(censoredValue);
                }
            }

            var record = new CompoundResidueCollection() {
                Food = food,
                Compound = substance,
                Positives = positives,
                CensoredValuesCollection = censoredValuesCollection,
                ZerosCount = zerosCount
            };
            return record;
        }

        /// <summary>
        /// Creates concentrations based on mu and sigmas
        /// </summary>
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
