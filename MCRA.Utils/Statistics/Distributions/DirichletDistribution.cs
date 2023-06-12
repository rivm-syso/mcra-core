using MathNet.Numerics.Distributions;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// DirichletDistribution
    /// </summary>
    public class DirichletDistribution {

        /// <summary>
        /// Draw values as a Dirichlet distribution, in which the returned values add up to 1.
        /// The returned individual values are distributed according to the input array of probabilities.
        /// </summary>
        /// <param name="probabilities">The probability distribution of the returned values</param>
        /// <param name="seed">The seed for the random generator</param>
        /// <returns>The random sample of values that add to 1</returns>
        public static double[] Sample(double[] probabilities, int seed) {
            //use a wrapper class to wrap a Troschuetz random generator
            //The dirichlet function expects the System.Random type
            var r = new McraRandomGenerator(seed);
            return Dirichlet.Sample(r, probabilities);
        }
    }
}
