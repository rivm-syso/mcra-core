using MCRA.General;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// No transformation is used
    /// </summary>
    public class IdentityTransformer : IntakeTransformer {

        public override TransformType TransformType => TransformType.NoTransform;

        public override double Transform(double x) {
            return x;
        }

        public override double InverseTransform(double x) {
            return x;
        }

        /// <summary>
        /// Transforms a BLUP (on the linear scale) to the original scale, but no
        /// transformation is applied.
        /// </summary>
        public override double BiasCorrectedInverseTransform(double x, double varianceWithin) {
            return x;
        }
    }
}
