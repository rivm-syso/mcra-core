using MathNet.Numerics.Distributions;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from Triangular distribution
    /// </summary>
    public class TriangularDistribution : Distribution {

        public double Lower { get; private set; }
        public double Upper { get; private set; }
        public double Mode { get; private set; }
        public double Mean { get; private set; }
        public double Variance { get; private set; }


        public TriangularDistribution(double lower, double upper, double mode) {
            Lower = lower;
            Upper = upper;
            Mode = mode;
            Mean = (lower + upper + mode) / 3.0;
            Variance = (lower * lower + upper * upper + mode * mode - lower * upper - lower * mode - upper * mode) / 18.0;
        }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public override double Draw(IRandom random) {
            var rnd = new RandomAsRandomWrapper(random);
            return Triangular.Sample(rnd, Lower, Upper, Mode);
        }
        public static List<double> Samples(IRandom random, double lower, double upper, double mode, int n) {
            var rnd = new RandomAsRandomWrapper(random);
            var result = new List<double>();
            for (int i = 0; i < n; i++) {
                result.Add(Triangular.Sample(rnd, lower, upper, mode));
            }
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="TriangularDistribution"/> instance based on a provided
        /// mode, lower and upper bound.
        /// </summary>
        public static TriangularDistribution FromModeLowerandUpper(double mode, double lower, double upper) {
            if (upper < mode) {
                var msg = $"Specified mode of {mode} is greater than the upper {upper}.";
                throw new ArgumentException(msg);
            } else if (lower > mode) {
                var msg = $"Specified mode of {mode} is smaller than the lower {lower}.";
                throw new ArgumentException(msg);
            }
            return new TriangularDistribution(lower, upper, mode);
        }

        public static double CDF(double lower, double upper, double mode, double x) {
            return Triangular.CDF(lower, upper, mode, x);
        }

        public static double PDF(double lower, double upper, double mode, double x) {
            return Triangular.PDF(lower, upper, mode, x);
        }

        public static double InvCDF(double lower, double upper, double mode, double p) {
            return Triangular.InvCDF(lower, upper, mode, p);
        }

        public static double Density(double lower, double upper, double mode, double x) {
            var triangular = new Triangular(lower, upper, mode);
            return triangular.Density(x);
        }

        public static List<double> TriangularSamples(int n, double lower, double upper, double mode) {
            var x = new double[n];
            Triangular.Samples(x, lower, upper, mode);
            return [.. x];
        }


        public override double CDF(double x) {
            return CDF(Lower, Upper, Mode, x);
        }
    }
}
