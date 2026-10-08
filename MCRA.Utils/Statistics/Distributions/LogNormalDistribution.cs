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
        /// and upper (p95) percentile. This function can be used to fit a lognormal distribution
        /// from a sample mean and p95 percentile provided on the original (untransformed) scale.
        /// Sampling from this fitted distribution should yield the same mean and p95 percentile
        /// as provided as input of this function. Because there can be two solutions (i.e., two
        /// distinct parametrisations of mu and sigma), there is a choice to be made in deciding
        /// which of the parametrisations to return. By default, the solution with the smallest
        /// sigma is returned, unless specified otherwise.
        /// </summary>
        /// <param name="mean">Mean on the original scale. Mean = exp(mu+1/2*sigma^2). </param>
        /// <param name="upper">Upper (p95) percentile (original scale). Upper = exp(mu + 1.645*sigma).</param>
        /// <param name="useSigmaHigh">If true, the solution/parametrisation with the highest sigma is returned.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static LogNormalDistribution 
            FromMeanAndUpper(double mean, double upper, bool useSigmaHigh = false) {
            if (double.IsNaN(upper)) {
                var msg = $"No upper p95 percentile for the lognormal distribution is specified.";
                throw new ArgumentException(msg);
            }

            var logMean = UtilityFunctions.LogBound(mean);
            var logUpper = UtilityFunctions.LogBound(upper);

            // Solve: 0.5*sigma^2 - 1.645*sigma + (logUpper - logMean) = 0
            // abc-formula: a = 0.5, b = -1.645, c = logUpper - logMean
            // (b * b) * 0.5 - 4 * a * c; 
            var a = 0.5;
            var b = -NormalDistribution.InvCDF(0, 1, 0.95);
            var c = logUpper - logMean;

            var discriminant = (b * b) - 4 * a * c;
            if (discriminant < 0) {
                throw new ArgumentException("The given combination of mean and upper p95 is mathematically impossible for a lognormal distribution.");
            }

            // Take the negative variant because sigma must be the smallest positive root
            var sigma = useSigmaHigh
                ? (-b + Math.Sqrt(discriminant)) / (2 * a)
                : (-b - Math.Sqrt(discriminant)) / (2 * a);

            var mu = logMean - (sigma * sigma) / 2.0;
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
