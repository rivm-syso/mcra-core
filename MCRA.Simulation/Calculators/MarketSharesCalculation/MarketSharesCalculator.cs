using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.MarketSharesCalculation {
    public class MarketSharesCalculator {

        /// <summary>
        /// Draw values as a Dirichlet distribution, in which the returned values add up to 1.
        /// Fallback is multinomial for no brandloyalty (L = 0, S = infinity) and the 
        /// probabilities itself for absolute brandloyalty (L = 1, S = 0)
        /// </summary>
        /// <param name="probabilities"></param>
        /// <param name="brandLoyalty"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static double[] SampleBrandLoyalty(
            IEnumerable<double> probabilities, 
            double brandLoyalty,
            int seed
        ) {
            var probs = probabilities.ToArray();

            // Check whether market share (fractions) add up to 1
            var probsSum = probs.Sum();
            var isIncomplete = probsSum < 1;
            if (isIncomplete) {
                // Incomplete shares, add market-share for remainder
                probs = probs.Append(1 - probabilities.Sum()).ToArray();
            } else if (probsSum > 1) {
                // Total market shares too high, rescale to sum up to 1
                probs = probs.Select(r => r / probsSum).ToArray();
            }

            double[] result;
            var r = new McraRandomGenerator(seed);
            if (brandLoyalty < 0.00001) {
                result = probs;
            } else if (brandLoyalty > .99) {
                var shares = MultinomialDistribution.Sample(probs, seed, 1);
                result = shares.Select(c => (double)c).ToArray();
            } else {
                var s = 1 / brandLoyalty - 1;
                probs = probs.Select(r => s * r).ToArray();
                result = DirichletDistribution.Sample(probs, seed);
            }

            if (isIncomplete) {
                // If we added an artificial record, then remove the draw for
                // the artificially added element.
                result = result.SkipLast(1).ToArray();
            }

            return result;
        }
    }
}
