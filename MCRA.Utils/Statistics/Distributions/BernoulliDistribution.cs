using MathNet.Numerics.Distributions;
using System.Collections.Generic;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from Bernoulli distribution
    /// </summary>
    public class BernoulliDistribution : Distribution {

        public double P { get; private set; }

        public BernoulliDistribution(double p) {
            P = p;
        }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <returns>A random draw from the distribution.</returns>
        public override double Draw(IRandom random) {
            var rnd = new RandomAsRandomWrapper(random);
            return Bernoulli.Sample(rnd, P);
        }

        public static double Draw(IRandom random, double p) {
            var rnd = new RandomAsRandomWrapper(random);
            return Bernoulli.Sample(rnd, p);
        }

        public static List<double> Samples(IRandom random, double p, int n) {
            var list = new List<double>();
            for (int i = 0; i < n; i++) {
                list.Add(Draw(random, p));
            }
            return list;
        }
    }
}
