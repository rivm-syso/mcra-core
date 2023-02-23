using MathNet.Numerics.Distributions;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from LogStudentT distribution
    /// </summary>
    public class LogStudentTScaledDistribution : Distribution {

        public double Location { get; private set; }
        public double Scale { get; private set; }
        public double Freedom { get; private set; }
        public double Offset { get; private set; }


        public LogStudentTScaledDistribution(double location,  double scale, double freedom, double offset = 0) {
            Location = location;
            Freedom = freedom;
            Scale = scale;
            Offset = offset;
        }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public override double Draw(IRandom random) {
            var rnd = new RandomAsRandomWrapper(random);
            var studentT = new StudentT(Location, Scale, Freedom, rnd);
            return Math.Exp(studentT.Sample()) + Offset;
        }

        public static double Draw(IRandom random, double location, double scale, double freedom, double offset = 0) {
            var rnd = new RandomAsRandomWrapper(random);
            return Math.Exp(StudentT.Sample(rnd, location, scale, freedom)) + offset;
        }

        public static List<double> Samples(IRandom random, double location, double scale, double freedom, int n, double offset = 0) {
            var list = new List<double>();
            for (int i = 0; i < n; i++) {
                list.Add(Draw(random, location, scale, freedom, offset));
            }
            return list;
        }

        public static double Density(double x, double location, double scale, double freedom) {
            var student = new StudentT(location, scale, freedom);
            return student.Density(x);
        }
    }
}
