namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from Normal distribution
    /// </summary>
    public class UniformDistribution : Distribution {

        public double Lower { get; private set; }
        public double Upper { get; private set; }

        public UniformDistribution(double lower, double upper) {
            Lower = lower;
            Upper = upper;
        }

        public override double Draw(IRandom random) {
            return Draw(random, Lower, Upper);
        }

        public static double Draw(IRandom random, double lower, double upper) {
            return random.NextDouble(lower, upper);
        }

        public static List<double> Samples(IRandom random, double lower, double upper, int n, double offset = 0) {
            var result = new List<double>();
            for (int i = 0; i < n; i++) {
                result.Add(Draw(random, lower, upper));
            }
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="UniformDistribution"/> instance based on a provided
        /// mean and upper bound.
        /// </summary>
        public static UniformDistribution FromMeanAndUpper(double mean, double upper) {
            if (upper < mean) {
                var msg = $"Specified mean of {mean} is greater than the upper {upper}.";
                throw new ArgumentException(msg);
            } else if (upper == mean) {
                var msg = $"Specified mean of {mean} is equal to the upper {upper}.";
                throw new ArgumentException(msg);
            }
            var lower = mean - (upper - mean);
            return new UniformDistribution(lower, upper);
        }

        /// <summary>
        /// Creates a new <see cref="UniformDistribution"/> instance based on a provided
        /// median and upper bound.
        /// </summary>
        public static UniformDistribution FromMedianAndUpper(double median, double upper) {
            if (upper < median) {
                var msg = $"Specified median of {median} is greater than the upper {upper}.";
                throw new ArgumentException(msg);
            } else if (upper == median) {
                var msg = $"Specified median of {median} is equal to the upper {upper}.";
                throw new ArgumentException(msg);
            }
            var lower = median - (upper - median);
            return new UniformDistribution(lower, upper);
        }

        /// <summary>
        /// Creates a new <see cref="UniformDistribution"/> instance based on a provided
        /// mean and CV.
        /// </summary>
        public static UniformDistribution FromMeanAndCv(double mean, double cv) {
            var lower = mean - Math.Sqrt(3) * mean * cv;
            var upper = mean + Math.Sqrt(3) * mean * cv;
            return new UniformDistribution(lower, upper);
        }

        public override double CDF(double x) {
            return (x - Lower) / (Upper - Lower);
        }
    }
}
