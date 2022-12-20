using MathNet.Numerics.Distributions;
using System.Collections.Generic;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from ChiSquare distribution
    /// </summary>
    public class ChiSquaredDistribution : Distribution {
        public double Freedom { get; private set; }
        public ChiSquaredDistribution(double freedom, bool obsoleteParameter = false) {
            Freedom = freedom;
        }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public override double Draw(IRandom random) {
            var rnd = new RandomAsRandomWrapper(random);
            return ChiSquared.Sample(rnd, Freedom);
        }

        public static double Draw(IRandom random, double freedom) {
            var rnd = new RandomAsRandomWrapper(random);
            return ChiSquared.Sample(rnd, freedom);
        }

        public static List<double> Samples(IRandom random, double freedom, int n) {
            var list = new List<double>();
            for (int i = 0; i < n; i++) {
                list.Add(Draw(random, freedom));
            }
            return list;
        }

        public static double InvCDF(double freedom, double p) {
            return ChiSquared.InvCDF(freedom, p);
        }

        public static double CDF(double freedom, double p) {
            return ChiSquared.CDF(freedom, p);
        }
    }
}

