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

        /// <summary>
        /// Creates a new <see cref="UniformDistribution"/> instance based on a provided
        /// mean and upper bound.
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
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
        /// <param name="median"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
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
    }
}
