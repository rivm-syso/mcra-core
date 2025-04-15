using MathNet.Numerics.Distributions;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from Gamma distribution
    /// </summary>
    public class GammaDistribution : Distribution {

        public double Shape { get; private set; }
        public double Rate { get; private set; }
        public double Scale { get; private set; }


        public GammaDistribution(double shape, double rate, double scale = 0) {
            Shape = shape;
            Rate = rate;
            Scale = scale;
        }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public override double Draw(IRandom random) {
            var rnd = new RandomAsRandomWrapper(random);
            return Gamma.Sample(rnd, Shape, Rate) + Scale;
        }


        public static double Draw(IRandom random, double shape, double rate, double scale = 0) {
            var rnd = new RandomAsRandomWrapper(random);
            return Gamma.Sample(rnd, shape, rate) + scale;
        }

        public static List<double> Samples(IRandom random, double shape, double rate, int n, double scale = 0) {
            var list = new List<double>();
            for (int i = 0; i < n; i++) {
                list.Add(Draw(random, shape, rate) + scale);
            }
            return list;
        }

        public static double Density(double x, double shape, double rate) {
            var gamma = new Gamma(shape, rate);
            return gamma.Density(x);
        }

        public override double CDF(double x) {
            throw new NotImplementedException();
        }
    }
}
