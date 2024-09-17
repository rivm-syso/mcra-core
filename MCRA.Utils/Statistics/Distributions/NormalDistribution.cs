using MathNet.Numerics.Distributions;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from Normal distribution
    /// </summary>
    public class NormalDistribution : Distribution {

        public double Mu { get; private set; }
        public double Stddev { get; private set; }

        public NormalDistribution(double mu, double stddev) {
            Stddev = stddev;
            Mu = mu;
        }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <returns>A random draw from the distribution.</returns>
        public override double Draw(IRandom random) {
            var rnd = new RandomAsRandomWrapper(random);
            return Normal.Sample(rnd, Mu, Stddev);
        }

        public static double Draw(IRandom random, double mu, double stddev) {
            var rnd = new RandomAsRandomWrapper(random);
            return Normal.Sample(rnd, mu, stddev);
        }

        public static List<double> Samples(IRandom random, double mu, double stddev, int n) {
            var rnd = new RandomAsRandomWrapper(random);
            var x = new double[n];
            Normal.Samples(rnd, x, mu, stddev);
            return x.ToList();
        }

        public static List<double> NormalSamples(int n, double mu, double sigma) {
            var x = new double[n];
            Normal.Samples(x, mu, sigma);
            return x.ToList();
        }

        public static double DrawInvCdf(IRandom random, double mu, double stddev) {
            return stddev * Normal.InvCDF(0, 1, random.NextDouble()) + mu;
        }

        public static double InvCDF(double mu, double stddev, double p) {
            return stddev * Normal.InvCDF(0, 1, p) + mu;
        }

        public static double PDF(double mu, double stddev, double x) {
            return Normal.PDF(mu, stddev, x);
        }

        public static double CDF(double mu, double stddev, double x) {
            return Normal.CDF(mu, stddev, x);
        }
    }
}
