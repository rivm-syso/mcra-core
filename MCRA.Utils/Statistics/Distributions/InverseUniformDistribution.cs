using MathNet.Numerics;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Implementation of the inverse uniform distribution.
    /// Parametrised as U_inf(a,b) = 1 / U(a,b).
    /// See https://en.wikipedia.org/wiki/Inverse_distribution
    /// </summary>
    public class InverseUniformDistribution : Distribution {

        public double A { get; private set; }
        public double B { get; private set; }

        /// <summary>
        /// Creates a new inverse uniform distribution instance
        /// parametrised as U_inf(a,b) = 1 / U(a,b).
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public InverseUniformDistribution(double a, double b) {
            A = a;
            B = b;
        }

        /// <summary>
        /// Draw from the distribution using the provided random generator.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public override double Draw(IRandom random) {
            return Draw(random, A, B);
        }

        /// <summary>
        /// Draws from a inverse uniform distribution instance parametrised as
        /// U_inf(a,b) = 1 / U(a,b).
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static double Draw(IRandom random, double a, double b) {
            var draw = 1 / random.NextDouble(a, b);
            return draw;
        }

        /// <summary>
        /// Creates a new <see cref="InverseUniformDistribution"/> instance based on a provided
        /// mean and upper boundary.
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static InverseUniformDistribution FromMeanAndUpper(double mean, double upper) {
            var a = 1D / upper;
            var b_median = 2 / mean - a;
            Func<double, double> equation =
            (x) => {
                var b_hat = mean * x - Math.Log(x) - mean * a + Math.Log(a);
                return b_hat;
            };
            var b = FindRoots.OfFunction(equation, b_median, 1e5);
            return new InverseUniformDistribution(a, b);
        }

        /// <summary>
        /// Creates a new <see cref="InverseUniformDistribution"/> instance based on a provided
        /// median and upper boundary.
        /// </summary>
        /// <param name="median"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public static InverseUniformDistribution FromMedianAndUpper(double median, double upper) {
            var a = 1 / upper;
            var b = 2 / median - a;
            return new InverseUniformDistribution(a, b);
        }
        public override double CDF(double x) {
            throw new NotImplementedException();
        }
    }
}
