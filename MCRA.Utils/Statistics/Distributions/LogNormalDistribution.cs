using MathNet.Numerics.Distributions;
using System.Collections.Generic;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from Gamma distribution
    /// </summary>
    public class LogNormalDistribution : Distribution {

        public double Mu { get; private set; }
        public double Sigma { get; private set; }
        public double Offset { get; private set; }


        public LogNormalDistribution(double mu, double sigma, double offset = 0) {
            Mu = mu;
            Sigma = sigma;
            Offset = offset;
        }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public override double Draw(IRandom random) {
            var rnd = new RandomAsRandomWrapper(random);
            return LogNormal.Sample(rnd, Mu, Sigma) + Offset;
        }

        public static double Draw(IRandom random, double mu, double sigma, double offset = 0) {
            var rnd = new RandomAsRandomWrapper(random);
            return LogNormal.Sample(rnd, mu, sigma) + offset;
        }

        public static List<double> Samples(IRandom random, double mu, double sigma, int n, double offset = 0) {
            var list = new List<double>();
            for (int i = 0; i < n; i++) {
                list.Add(Draw(random, mu, sigma, offset));
            }
            return list;
        }
        public static double InvCDF(double mu, double sigma, double p) {
            return LogNormal.InvCDF(mu, sigma, p);
        }
        public static double Density(double x, double mu, double sigma) {
            var lognormal = new LogNormal(mu, sigma);
            return lognormal.Density(x);
        }
        public static List<double> LogNormalSamples(int n, double mu, double sigma) {
            var x = new double[n];
            LogNormal.Samples(x, mu, sigma);
            return x.ToList();
        }
    }
}
