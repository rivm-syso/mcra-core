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

        public override double CDF(double x) {
            return CDF(Mu, Sigma, x);
        }

        public double InvCDF(double p) {
            return InvCDF(Mu, Sigma, p);
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

        /// <summary>
        /// Fits a <see cref="LogNormalDistribution"/> from a list of percentile points,
        /// specified as a list of (percentage, value) value tuples.
        /// </summary>
        public static LogNormalDistribution FitFromPercentiles(List<(double Percentage, double Value)> points) {
            if (points.Count < 2) {
                throw new ArgumentException("Cannot compute lognormal distribution from fewer than 2 percentile points.");
            }
            if (points.Any(r => r.Percentage <= 0 || r.Percentage >= 100)) {
                throw new ArgumentException("Cannot compute lognormal distribution from percentile points: percentiles should be between 0 and 100.");
            }
            if (points.All(r => r.Value <= 0)) {
                throw new ArgumentException("Cannot compute lognormal distribution from percentile points with values <= 0.");
            }

            // Transform to (z, y) = (probit(p/100), ln(value)) and fit y = mu + sigma * z.
            var z = points.Select(p => Normal.InvCDF(0, 1, p.Percentage / 100.0)).ToArray();
            var y = points.Select(p => Math.Log(p.Value)).ToArray();

            // Ordinary least squares fit of y = a + b * x, returning (a, b).
            // For exactly two points this reduces to the exact line through them.
            var n = z.Length;
            var meanX = z.Average();
            var meanY = y.Average();

            double num = 0, den = 0;
            for (var i = 0; i < n; i++) {
                num += (z[i] - meanX) * (y[i] - meanY);
                den += (z[i] - meanX) * (z[i] - meanX);
            }

            // Set mu is intercept and sigma is slope
            var sigma = num / den;
            var mu = meanY - sigma * meanX;

            var distribution = new LogNormalDistribution(mu, sigma);
            return distribution;
        }
    }
}
