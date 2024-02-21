using MathNet.Numerics.Distributions;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// MultinomialDistribution
    /// </summary>
    public class MultinomialDistribution {

        /// <summary>
        /// Draw values from a multinomial distribution. The returned individual values are 
        /// distributed according to the input array of probabilities.
        /// </summary>
        /// <param name="probabilities">The probability distribution of the returned values</param>
        /// <param name="seed">The seed for the random generator</param>
        /// <returns>The random sample of values that add to 1</returns>
          public static int[] Sample(double[] probabilities, int seed, int number) {
            var r = new McraRandomGenerator(seed);
            return Multinomial.Sample(r, probabilities, number);
        }
    }
}
