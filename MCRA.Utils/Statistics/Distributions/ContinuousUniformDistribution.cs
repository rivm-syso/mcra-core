using MathNet.Numerics.Distributions;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from ContinuousUniform distribution
    /// </summary>
    public class ContinuousUniformDistribution : Distribution {

        public double Lower { get; private set; }
        public double Upper { get; private set; }

        public ContinuousUniformDistribution(double lower, double upper) {
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
            return ContinuousUniform.Sample(rnd, Lower, Upper);
        }

        public static double Draw(IRandom random, double lower, double upper) {
            var rnd = new RandomAsRandomWrapper(random);
            return ContinuousUniform.Sample(rnd, lower, upper);
        }

        public static List<double> Samples(IRandom random, double lower, double upper, int n) {
            var list = new List<double>();
            for (int i = 0; i < n; i++) {
                list.Add(Draw(random, lower, upper));
            }
            return list;
        }
        public static double Draw(double lower, double upper) {
            return ContinuousUniform.Sample(lower, upper);
        }
    }
}
