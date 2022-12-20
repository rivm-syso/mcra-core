using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.IntakeModelling.Integrators {
    public sealed class TransformPower : TransformIdentity{

        /// <summary>
        /// The power parameter.
        /// </summary>
        public double Power { get; set; }

        /// <summary>
        /// The Gauss-Hermite points of this distribution.
        /// </summary>
        public double[,] GaussHermitePoints { get; set; }

        /// <summary>
        /// Draws from the distribution using the given random number generator.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <returns>A random draw from the distribution.</returns>
        public override double Draw(IRandom random) {
            var z = UtilityFunctions.BackTransformAmountBoxCox(base.Draw(random), VarianceWithin, Power, GaussHermitePoints);
            if (double.IsNaN(z)) {
                z = 0;
            }
            return z;
        }
    }
}
