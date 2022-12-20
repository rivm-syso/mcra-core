using MathNet.Numerics.Distributions;
using System.Collections.Generic;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from DiscreteUniform distribution
    /// </summary>
    public class DiscreteUniformDistribution : Distribution {

        public int Lower { get; private set; }
        public int Upper { get; private set; }


        public DiscreteUniformDistribution(int lower, int upper) {
            Lower = lower;
            Upper = upper;
        }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public override double Draw(IRandom random) {
            var rnd = new RandomAsRandomWrapper(random);
            return DiscreteUniform.Sample(rnd, Lower, Upper);
        }

        public static double Draw(IRandom random, int lower, int upper) {
            var rnd = new RandomAsRandomWrapper(random);
            return DiscreteUniform.Sample(rnd, lower, upper);
        }

        public static List<double> Samples(IRandom random, int lower, int upper, int n) {
            var list = new List<double>();
            for (int i = 0; i < n; i++) {
                list.Add(Draw(random, lower, upper));
            }
            return list;
        }
    }
}
