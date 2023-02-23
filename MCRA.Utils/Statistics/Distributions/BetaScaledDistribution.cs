using MathNet.Numerics.Distributions;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from BetaScaled distribution
    /// </summary>
    public class BetaScaledDistribution : Distribution {

        public double ShapeA { get; private set; }
        public double ShapeB { get; private set; }
        public double Location { get; private set; }
        public double Scale { get; private set; }


        public BetaScaledDistribution(double shapeA, double shapeB, double location, double scale) {
            ShapeA = shapeA;
            ShapeB = shapeB;
            Scale = scale;
            Location = location;
        }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public override double Draw(IRandom random) {
            var rnd = new RandomAsRandomWrapper(random);
            return BetaScaled.Sample(rnd, ShapeA, ShapeB, Location, Scale);
        }

        public static double Draw(IRandom random, double shapeA, double shapeB, double location, double scale) {
            var rnd = new RandomAsRandomWrapper(random);
            return BetaScaled.Sample(rnd, shapeA, shapeB, location, scale);
        }

        public static List<double> Samples(IRandom random, double shapeA, double shapeB, double location, double scale, int n) {
            var list = new List<double>();
            for (int i = 0; i < n; i++) {
                list.Add(Draw(random, shapeA, shapeB, location, scale));
            }
            return list;
        }

    }
}
