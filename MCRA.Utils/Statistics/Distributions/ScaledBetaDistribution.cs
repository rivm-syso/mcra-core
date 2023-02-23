using MathNet.Numerics.Distributions;

namespace MCRA.Utils.Statistics {

    /// <summary>
    /// Random draw from Gamma distribution
    /// </summary>
    public class ScaledBetaDistribution {

        private readonly double _A ;
        private readonly double _B ;
        private readonly double _C;
        private readonly double _D;
        private readonly Beta _beta;

        public ScaledBetaDistribution(double a, double b, double c, double d, Random random) {
            _A = a;
            _B = b;
            _C = c;
            _D = d;
            _beta = new Beta(_A, _B, random);
        }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public double Draw(Random random) {
            return _C + _beta.Sample() * (_D - _C);
        }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public List<double> Draw(Random random, int n = 1000) {
            var draws = new List<double>();
            for (int i = 0; i < 1000; i++) {
                draws.Add(Draw(random));
            }
            return draws;
        }
    }
}
