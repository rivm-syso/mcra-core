using MathNet.Numerics.Distributions;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from StudentT distribution
    /// </summary>
    public class StudentTDistribution : Distribution {

        public double Location { get; private set; }
        public double Scale { get; private set; }
        public double Freedom { get; private set; }


        public StudentTDistribution(double location,  double scale, double freedom) {
            Location = location;
            Freedom = freedom;
            Scale = scale;
        }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public override double Draw(IRandom random) {
            var rnd = new RandomAsRandomWrapper(random);
            var studentT = new StudentT(Location, Scale, Freedom, rnd);
            return studentT.Sample();
        }

        public static double Draw(IRandom random, double location, double scale, double freedom) {
            var rnd = new RandomAsRandomWrapper(random);
            return StudentT.Sample(rnd, location, scale, freedom);
        }

        public static List<double> Samples(IRandom random, double location, double scale, double freedom, int n) {
            var list = new List<double>();
            for (int i = 0; i < n; i++) {
                list.Add(Draw(random, location, scale, freedom));
            }
            return list;
        }

        public static double Density(double x, double location, double scale, double freedom) {
            var student = new StudentT(location, scale, freedom);
            return student.Density(x);
        }
    }
}
