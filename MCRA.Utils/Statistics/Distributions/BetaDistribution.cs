using MathNet.Numerics.Distributions;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from Beta distribution
    /// </summary>
    public class BetaDistribution : Distribution {
        public double ShapeA { get; private set; }
        public double ShapeB { get; private set; }

        public BetaDistribution(double shapeA, double shapeB) {
            ShapeA = shapeA;
            ShapeB = shapeB;
        }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public override double Draw(IRandom random) {
            var rnd = new RandomAsRandomWrapper(random);
            return Beta.Sample(rnd, ShapeA, ShapeB);
        }

        public static double Draw(IRandom random, double shapeA, double shapeB) {
            var rnd = new RandomAsRandomWrapper(random);
            return Beta.Sample(rnd, shapeA, shapeB);
        }

        public static List<double> Samples(IRandom random, double shapeA, double shapeB, int n) {
            var list = new List<double>();
            for (int i = 0; i < n; i++) {
                list.Add(Draw(random, shapeA, shapeB));
            }
            return list;
        }

        public static double InvCDF(double shapeA, double shapeB, double p) {
            return Beta.InvCDF(shapeA, shapeB, p);
        }

        public static double CDF(double shapeA, double shapeB, double p) {
            return Beta.CDF(shapeA, shapeB, p);
        }

        public static double Density(double x, double shapeA, double shapeB) {
            var beta = new Beta(shapeA, shapeB);
            return beta.Density(x);
        }

        /// <summary>
        /// Creates a new <see cref="BetaDistribution"/> instance based on a provided mean and
        /// a variance.
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="variance"></param>
        /// <returns></returns>
        public static BetaDistribution FromMeanAndVariance(double mean, double variance) {
            var alpha = ((1 - mean) / variance - 1 / mean) * mean * mean;
            var beta = alpha * (1 / mean - 1);
            return new BetaDistribution(alpha, beta);
        }

        public override double CDF(double x) {
            return CDF(ShapeA, ShapeB, x);
        }
    }
}
