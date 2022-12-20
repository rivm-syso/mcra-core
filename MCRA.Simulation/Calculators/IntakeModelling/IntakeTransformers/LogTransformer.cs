using MCRA.Utils;
using MCRA.General;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Transforms a set of positive daily exposures to normality based on a natural logarithm
    /// </summary>
    public class LogTransformer : IntakeTransformer {

        public override TransformType TransformType => TransformType.Logarithmic;

        public override double Transform(double x) {
            return x == 0 ? 0 : UtilityFunctions.LogBound(x);
        }

        public override double InverseTransform(double x) {
            return x == 0 ? 0 : UtilityFunctions.ExpBound(x);
        }

        /// <summary>
        /// Backtransforms a BLUP (on the linear scale) to the original scale based on a log transformation
        /// </summary>
        public override double BiasCorrectedInverseTransform(double x, double varianceWithin) {
            return UtilityFunctions.ExpBound(x + varianceWithin / 2);
        }
    }
}
