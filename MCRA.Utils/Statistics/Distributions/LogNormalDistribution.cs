using MathNet.Numerics.Distributions;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from Gamma distribution
    /// </summary>
    public class LogNormalDistribution : Distribution {

        public double Mu { get; private set; }
        public double Sigma { get; private set; }
        public double Offset { get; private set; } = 0;

        public LogNormalDistribution(double mu, double sigma) {
            Mu = mu;
            Sigma = sigma;
        }

        public LogNormalDistribution(double mu, double sigma, double offset) : this(mu, sigma) {
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
            var result = new List<double>();
            for (int i = 0; i < n; i++) {
                result.Add(Draw(random, mu, sigma, offset));
            }
            return result;
        }

        public static double CDF(double mu, double sigma, double x) {
            return LogNormal.CDF(mu, sigma, x);
        }

        public static double PDF(double mu, double sigma, double x) {
            return LogNormal.PDF(mu, sigma, x);
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

        /// <summary>
        /// Creates a <see cref="LogNormalDistribution"/> instance based on a provided mean
        /// and upper (p95) percentile.
        /// </summary>
        public static LogNormalDistribution FromMeanAndUpper(double mean, double upper) {
            var mu = UtilityFunctions.LogBound(mean);
            if (mean > upper) {
                var msg = $"The provided mean of {mean} is higher than the specified upper p95 percentile of {upper}.";
                throw new ArgumentException(msg);
            }
            var sigma = (UtilityFunctions.LogBound(upper) - mu) / 1.645;
            return new LogNormalDistribution(mu, sigma);
        }

        /// <summary>
        /// Creates a <see cref="LogNormalDistribution"/> instance based on a provided mean
        /// and upper (p95) percentile.
        /// </summary>
        public static LogNormalDistribution FromMeanAndCv(double mean, double cv) {
            var sigma = Math.Sqrt(Math.Log(Math.Pow(cv, 2) + 1));
            var mu = Math.Log(mean) - Math.Pow(sigma, 2) / 2;
            var distribution = new LogNormalDistribution(mu, sigma);
            return distribution;
        }
    }
}
